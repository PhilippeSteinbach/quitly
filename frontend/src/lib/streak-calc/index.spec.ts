import { describe, it, expect, beforeEach, vi } from 'vitest';
import fc from 'fast-check';
import {
  calculateCurrentSeconds,
  calculateDayStatus,
  calculateMonthStats,
  calculateYearStats,
  detectManipulation,
} from './index';

// ── helpers ──────────────────────────────────────────────────────────────────

const MS = 1000;
const MIN = 60 * MS;
const HOUR = 60 * MIN;
const DAY = 24 * HOUR;

function utcMs(isoDate: string, time = '00:00:00'): number {
  return new Date(`${isoDate}T${time}Z`).getTime();
}

// ── calculateCurrentSeconds ──────────────────────────────────────────────────

describe('calculateCurrentSeconds', () => {
  it('returns seconds from startedAt when no relapses', () => {
    const started = utcMs('2026-01-01');
    const now = started + 3600 * MS;
    expect(calculateCurrentSeconds(started, [], now)).toBe(3600);
  });

  it('uses latest relapse as anchor', () => {
    const started = utcMs('2026-01-01');
    const r1 = started + DAY;
    const r2 = started + 3 * DAY;
    const now = r2 + 7200 * MS;
    expect(calculateCurrentSeconds(started, [r1, r2], now)).toBe(7200);
  });

  it('ignores relapses before startedAt', () => {
    const started = utcMs('2026-01-15');
    const before = utcMs('2026-01-10');
    const now = started + 500 * MS;
    expect(calculateCurrentSeconds(started, [before], now)).toBe(500);
  });

  it('returns 0 when now equals startedAt', () => {
    const started = utcMs('2026-01-01');
    expect(calculateCurrentSeconds(started, [], started)).toBe(0);
  });

  // Property: never negative
  it('property: never negative', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 0, max: 86400 * 365 }),
        fc.integer({ min: 0, max: 86400 * 365 }),
        (startOffset, nowOffset) => {
          const base = utcMs('2026-01-01');
          const started = base + startOffset * MS;
          const now = base + nowOffset * MS;
          return calculateCurrentSeconds(started, [], Math.max(started, now)) >= 0;
        }
      ),
      { numRuns: 300 }
    );
  });

  // Property: uniform time shift preserves seconds
  it('property: uniform shift preserves seconds', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 0, max: 86400 * 30 }),
        fc.integer({ min: 0, max: 86400 * 30 }),
        fc.integer({ min: -86400, max: 86400 }),
        (startOffset, duration, shiftSec) => {
          const base = utcMs('2026-01-01');
          const started = base + startOffset * MS;
          const now = started + duration * MS;
          const shift = shiftSec * MS;
          const baseline = calculateCurrentSeconds(started, [], now);
          const shifted  = calculateCurrentSeconds(started + shift, [], now + shift);
          return baseline === shifted;
        }
      ),
      { numRuns: 200 }
    );
  });

  // Property: adding a relapse never increases the streak
  it('property: relapse never increases streak', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 2, max: 86400 * 30 }),
        (duration) => {
          const started = utcMs('2026-01-01');
          const now = started + duration * MS;
          const midpoint = started + Math.floor(duration / 2) * MS;
          const without = calculateCurrentSeconds(started, [], now);
          const with_   = calculateCurrentSeconds(started, [midpoint], now);
          return with_ <= without;
        }
      ),
      { numRuns: 200 }
    );
  });
});

// ── calculateDayStatus ───────────────────────────────────────────────────────

describe('calculateDayStatus', () => {
  it('returns neutral for dates before startedAt', () => {
    const started = utcMs('2026-01-15');
    expect(calculateDayStatus('2026-01-14', 'UTC', started, [], '2026-01-15')).toBe('neutral');
  });

  it('returns abstinent on the start day with no relapse', () => {
    const started = utcMs('2026-01-15');
    expect(calculateDayStatus('2026-01-15', 'UTC', started, [], '2026-01-15')).toBe('abstinent');
  });

  it('returns neutral for future dates', () => {
    const started = utcMs('2026-01-01');
    expect(calculateDayStatus('2099-12-31', 'UTC', started, [], '2026-01-15')).toBe('neutral');
  });

  it('returns relapse when a relapse falls on that day', () => {
    const started = utcMs('2026-01-01');
    const relapse = utcMs('2026-02-01', '14:00:00');
    expect(calculateDayStatus('2026-02-01', 'UTC', started, [relapse], '2026-02-28')).toBe('relapse');
  });

  it('midnight UTC relapse belongs to the new day (UTC)', () => {
    const started = utcMs('2026-01-01');
    const midnight = utcMs('2026-02-01', '00:00:00');
    expect(calculateDayStatus('2026-02-01', 'UTC', started, [midnight], '2026-02-28')).toBe('relapse');
    expect(calculateDayStatus('2026-01-31', 'UTC', started, [midnight], '2026-02-28')).toBe('abstinent');
  });

  it('maps 22:00 UTC June 30 to July 1 in Europe/Berlin (UTC+2 summer)', () => {
    // 22:00 UTC = 00:00+02:00 Berlin (next calendar day)
    const started = utcMs('2026-06-01');
    const relapseUtc = utcMs('2026-06-30', '22:00:00');
    const today = '2026-07-01';

    expect(calculateDayStatus('2026-07-01', 'Europe/Berlin', started, [relapseUtc], today)).toBe('relapse');
    expect(calculateDayStatus('2026-06-30', 'Europe/Berlin', started, [relapseUtc], today)).toBe('abstinent');
  });
});

// ── calculateMonthStats ──────────────────────────────────────────────────────

describe('calculateMonthStats', () => {
  it('full abstinent month returns all days abstinent', () => {
    const started = utcMs('2026-02-01');
    const stats = calculateMonthStats(2026, 2, 'UTC', started, [], '2026-02-28');
    expect(stats.relevantDays).toBe(28);
    expect(stats.abstinentDays).toBe(28);
    expect(stats.relapseCount).toBe(0);
  });

  it('habit started mid-month caps relevantDays', () => {
    const started = utcMs('2026-05-15');
    const stats = calculateMonthStats(2026, 5, 'UTC', started, [], '2026-05-31');
    expect(stats.relevantDays).toBe(17); // 15–31 inclusive
  });

  it('current month caps denominator to today', () => {
    const started = utcMs('2026-05-01');
    const stats = calculateMonthStats(2026, 5, 'UTC', started, [], '2026-05-10');
    expect(stats.relevantDays).toBe(10);
    expect(stats.isCurrentMonth).toBe(true);
  });

  it('month entirely before startedAt returns zeros', () => {
    const started = utcMs('2026-06-01');
    const stats = calculateMonthStats(2026, 5, 'UTC', started, [], '2026-06-30');
    expect(stats.relevantDays).toBe(0);
    expect(stats.abstinentDays).toBe(0);
  });

  it('Feb 29 2028 (leap year) counted correctly', () => {
    const started = utcMs('2028-02-01');
    const stats = calculateMonthStats(2028, 2, 'UTC', started, [], '2028-02-29');
    expect(stats.relevantDays).toBe(29);
    expect(stats.abstinentDays).toBe(29);
  });

  it('DST spring-forward Berlin 2026-03-29 — no missing hour', () => {
    const started = utcMs('2026-03-01');
    const stats = calculateMonthStats(2026, 3, 'Europe/Berlin', started, [], '2026-03-29');
    expect(stats.relevantDays).toBe(29);
    expect(stats.abstinentDays).toBe(29);
  });

  it('DST fall-back Berlin 2026-10-25 — no duplicate day', () => {
    const started = utcMs('2026-10-01');
    const stats = calculateMonthStats(2026, 10, 'Europe/Berlin', started, [], '2026-10-25');
    expect(stats.relevantDays).toBe(25);
    expect(stats.abstinentDays).toBe(25);
  });

  it('counts individual relapse events and relapse days correctly', () => {
    const started = utcMs('2026-05-01');
    const relapses = [
      utcMs('2026-05-05', '10:00:00'),
      utcMs('2026-05-10', '10:00:00'),
      utcMs('2026-05-10', '18:00:00'), // same day
    ];
    const stats = calculateMonthStats(2026, 5, 'UTC', started, relapses, '2026-05-31');
    expect(stats.relevantDays).toBe(31);
    expect(stats.abstinentDays).toBe(29); // 31 - 2 relapse days
    expect(stats.relapseCount).toBe(3);
  });

  // Property: relevantDays ≤ daysInMonth
  it('property: relevantDays ≤ days-in-month', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 2025, max: 2030 }),
        fc.integer({ min: 1, max: 12 }),
        (year, month) => {
          const days = new Date(year, month, 0).getDate();
          const started = utcMs(`${year}-${String(month).padStart(2, '0')}-01`);
          const today = `${year}-${String(month).padStart(2, '0')}-${String(days).padStart(2, '0')}`;
          const stats = calculateMonthStats(year, month, 'UTC', started, [], today);
          return stats.relevantDays <= days;
        }
      ),
      { numRuns: 300 }
    );
  });
});

// ── calculateYearStats ───────────────────────────────────────────────────────

describe('calculateYearStats', () => {
  it('returns 12 months', () => {
    const started = utcMs('2026-01-01');
    const stats = calculateYearStats(2026, 'UTC', started, [], '2026-12-31');
    expect(stats.months).toHaveLength(12);
  });

  it('totals aggregate month sums', () => {
    const started = utcMs('2026-01-01');
    const stats = calculateYearStats(2026, 'UTC', started, [], '2026-12-31');
    const sumAbstinent = stats.months.reduce((s, m) => s + m.abstinentDays, 0);
    const sumRelevant  = stats.months.reduce((s, m) => s + m.relevantDays,  0);
    expect(stats.totalAbstinentDays).toBe(sumAbstinent);
    expect(stats.totalRelevantDays).toBe(sumRelevant);
  });
});

// ── detectManipulation ───────────────────────────────────────────────────────

describe('detectManipulation', () => {
  it('returns true when device clock was set back more than 5 min', () => {
    expect(detectManipulation(100_000, 500_000)).toBe(true); // server - offline = 400_000 > 300_000
  });

  it('returns false when difference is below threshold', () => {
    expect(detectManipulation(490_000, 500_000)).toBe(false); // 10_000 < 300_000
  });

  it('returns false for positive deviation (device ran fast)', () => {
    expect(detectManipulation(600_000, 500_000)).toBe(false);
  });

  it('returns false at exactly the threshold (strict >)', () => {
    expect(detectManipulation(200_000, 500_000)).toBe(false); // diff = 300_000 exactly
  });

  // Property: matches threshold formula exactly
  it('property: matches threshold formula', () => {
    fc.assert(
      fc.property(
        fc.integer({ min: 0, max: 999_999 }),
        fc.integer({ min: 0, max: 999_999 }),
        (offline, server) => {
          return detectManipulation(offline, server) === (server - offline > 300_000);
        }
      ),
      { numRuns: 500 }
    );
  });
});
