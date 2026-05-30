/**
 * Pure streak calculation functions — mirror of backend/src/Domain/Calculation/StreakCalculator.cs.
 * Zero external dependencies. All timezone handling uses Intl.DateTimeFormat with 'sv-SE' locale
 * to get a stable YYYY-MM-DD string that converts to DateOnly-equivalent comparisons.
 *
 * Constitution II (Non-shaming): DayStatus.paused is reserved but never returned in MVP.
 */

export type DayStatus = 'abstinent' | 'relapse' | 'paused' | 'neutral';

export interface MonthStats {
  abstinentDays: number;
  relevantDays: number;
  relapseCount: number;
  isCurrentMonth: boolean;
}

export interface YearStats {
  totalAbstinentDays: number;
  totalRelevantDays: number;
  months: MonthStats[];
}

// ── Helpers ──────────────────────────────────────────────────────────────────

/**
 * Returns a YYYY-MM-DD string for a UTC epoch in the user's timezone.
 * Uses 'sv-SE' locale because it produces ISO-8601 date format without extra setup.
 */
function toLocalDateString(utcMs: number, timezone: string): string {
  return new Intl.DateTimeFormat('sv-SE', { timeZone: timezone }).format(new Date(utcMs));
}

function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate(); // month is 1-based; day=0 = last day of previous month
}

function dateString(year: number, month: number, day: number): string {
  return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

// ── calculateCurrentSeconds ──────────────────────────────────────────────────

/**
 * Returns abstinence duration in UTC seconds since the last valid relapse
 * (or startedAt if no relapses occurred after startedAt).
 */
export function calculateCurrentSeconds(
  startedAtMs: number,
  relapseTimesMs: number[],
  nowMs: number
): number {
  const validRelapses = relapseTimesMs.filter(r => r >= startedAtMs);
  const anchor = validRelapses.length > 0
    ? Math.max(...validRelapses)
    : startedAtMs;

  return Math.max(0, Math.floor((nowMs - anchor) / 1000));
}

// ── calculateDayStatus ───────────────────────────────────────────────────────

/**
 * Returns the abstinence status of a local calendar date (YYYY-MM-DD string).
 *
 * @param todayDateStr  Optional explicit "today" in the user timezone (YYYY-MM-DD).
 *                      Pass this in tests to avoid coupling to Date.now().
 */
export function calculateDayStatus(
  dateDateStr: string,
  timezone: string,
  startedAtMs: number,
  relapseTimesMs: number[],
  todayDateStr?: string
): DayStatus {
  const startedAtLocal = toLocalDateString(startedAtMs, timezone);

  if (dateDateStr < startedAtLocal) return 'neutral';

  const today = todayDateStr ?? toLocalDateString(Date.now(), timezone);
  if (dateDateStr > today) return 'neutral';

  // 'paused' is never returned in MVP (Feature 005 stub)

  const hasRelapse = relapseTimesMs.some(
    r => toLocalDateString(r, timezone) === dateDateStr
  );
  return hasRelapse ? 'relapse' : 'abstinent';
}

// ── calculateMonthStats ──────────────────────────────────────────────────────

/**
 * Returns monthly abstinence statistics.
 * Denominator for the current month is capped to today (not the full month length).
 */
export function calculateMonthStats(
  year: number,
  month: number,
  timezone: string,
  startedAtMs: number,
  relapseTimesMs: number[],
  todayDateStr: string
): MonthStats {
  const monthDays = daysInMonth(year, month);
  const monthFirstStr = dateString(year, month, 1);
  const monthLastStr  = dateString(year, month, monthDays);

  const startedAtLocal = toLocalDateString(startedAtMs, timezone);
  const firstRelevantStr = startedAtLocal > monthFirstStr ? startedAtLocal : monthFirstStr;

  const todayYear  = parseInt(todayDateStr.slice(0, 4), 10);
  const todayMonth = parseInt(todayDateStr.slice(5, 7), 10);
  const isCurrentMonth = todayYear === year && todayMonth === month;

  const lastRelevantStr = (isCurrentMonth && todayDateStr < monthLastStr)
    ? todayDateStr
    : monthLastStr;

  if (firstRelevantStr > lastRelevantStr) {
    return { abstinentDays: 0, relevantDays: 0, relapseCount: 0, isCurrentMonth };
  }

  let abstinentDays = 0;
  let relevantDays = 0;

  // Iterate day-by-day
  const firstDay  = parseInt(firstRelevantStr.slice(8, 10), 10);
  const lastDay   = parseInt(lastRelevantStr.slice(8, 10), 10);

  for (let d = firstDay; d <= lastDay; d++) {
    const dateStr = dateString(year, month, d);
    relevantDays++;
    const status = calculateDayStatus(dateStr, timezone, startedAtMs, relapseTimesMs, todayDateStr);
    if (status === 'abstinent') abstinentDays++;
  }

  const relapseCount = relapseTimesMs.filter(r => {
    const rDate = toLocalDateString(r, timezone);
    return rDate >= firstRelevantStr && rDate <= lastRelevantStr;
  }).length;

  return { abstinentDays, relevantDays, relapseCount, isCurrentMonth };
}

// ── calculateYearStats ───────────────────────────────────────────────────────

/**
 * Returns yearly statistics — one MonthStats per calendar month (months 1–12).
 */
export function calculateYearStats(
  year: number,
  timezone: string,
  startedAtMs: number,
  relapseTimesMs: number[],
  todayDateStr: string
): YearStats {
  const months = Array.from({ length: 12 }, (_, i) =>
    calculateMonthStats(year, i + 1, timezone, startedAtMs, relapseTimesMs, todayDateStr)
  );
  return {
    totalAbstinentDays: months.reduce((s, m) => s + m.abstinentDays, 0),
    totalRelevantDays:  months.reduce((s, m) => s + m.relevantDays,  0),
    months,
  };
}

// ── detectManipulation ───────────────────────────────────────────────────────

/**
 * Returns true when the device clock appears to have been set back.
 * Client compares elapsed time from monotonic snapshot vs server delta.
 * Only triggers when server delta exceeds offline delta by more than 5 minutes (300 000 ms).
 * Positive deviation (device ran fast) is silently corrected — no user toast.
 */
export function detectManipulation(offlineDeltaMs: number, serverDeltaMs: number): boolean {
  return serverDeltaMs - offlineDeltaMs > 300_000;
}
