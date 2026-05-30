import type { GuestCheckIn } from "@/features/guest/guestStorage";

export type GuestStreak = {
  currentDays: number;
  longestDays: number;
};

/**
 * Pure function — no side effects.
 * Mirrors the streak logic used by the server StreakService.
 *
 * Rules:
 * - Only "abstinent" check-ins count toward the streak.
 * - A gap (no check-in on a day between the chain and today) breaks the streak.
 * - Check-ins after `today` are ignored.
 */
export function computeGuestStreak(
  checkIns: GuestCheckIn[],
  today: string, // YYYY-MM-DD in the user's timezone
  _timezone: string // reserved for future locale-aware comparisons
): GuestStreak {
  if (checkIns.length === 0) {
    return { currentDays: 0, longestDays: 0 };
  }

  // Filter to dates ≤ today and sort ascending
  const sorted = checkIns
    .filter((c) => c.date <= today)
    .sort((a, b) => a.date.localeCompare(b.date));

  if (sorted.length === 0) {
    return { currentDays: 0, longestDays: 0 };
  }

  // Walk backwards from today to compute current streak
  let currentDays = 0;
  let cursor = today;

  // Work backwards day-by-day
  for (let i = sorted.length - 1; i >= 0; i--) {
    const entry = sorted[i];

    if (entry.date === cursor) {
      if (entry.status === "abstinent") {
        currentDays++;
        cursor = subtractOneDay(cursor);
      } else {
        // Non-abstinent or unsure resets streak
        break;
      }
    } else if (entry.date < cursor) {
      // Gap between entry and cursor — streak broken
      break;
    }
  }

  // Compute longest streak across full history
  let longestDays = 0;
  let runLength = 0;
  let prevDate: string | null = null;

  for (const entry of sorted) {
    if (entry.status !== "abstinent") {
      longestDays = Math.max(longestDays, runLength);
      runLength = 0;
      prevDate = null;
      continue;
    }

    if (prevDate === null) {
      runLength = 1;
    } else if (entry.date === addOneDay(prevDate)) {
      runLength++;
    } else {
      // Gap
      longestDays = Math.max(longestDays, runLength);
      runLength = 1;
    }

    prevDate = entry.date;
  }

  longestDays = Math.max(longestDays, runLength);

  return { currentDays, longestDays };
}

// ─── Date helpers ─────────────────────────────────────────────────────────────

function subtractOneDay(dateStr: string): string {
  const date = new Date(`${dateStr}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() - 1);
  return date.toISOString().slice(0, 10);
}

function addOneDay(dateStr: string): string {
  const date = new Date(`${dateStr}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + 1);
  return date.toISOString().slice(0, 10);
}
