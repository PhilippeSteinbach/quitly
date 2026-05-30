# Data Model: 008 — Streak- und Abstinenz-Berechnung

**Phase**: 1 | **Date**: 2026-05-30 | **Branch**: `008-streak-calculation`

## Entity Changes

### 1. `Habit` — Add `StartedAt` (DateTimeOffset)

```
habits table
─────────────────────────────────────────────
  id               uuid  PK
  user_id          uuid  FK → users
  category         int
  mode             int   (0=Reduce, 1=Quit)
  title            varchar(120)
  active           bool
  started_on       date           ← KEEP for day-level compat (deprecated path)
  started_at       timestamptz    ← NEW: second-accurate start; NULL = migrated from started_on
  created_at       timestamptz
  updated_at       timestamptz
```

Migration: populate `started_at` from `started_on AT TIME ZONE 'UTC'` for existing rows.

---

### 2. `Streak` — Redesign (per-habit, UTC-seconds)

```
streaks table
─────────────────────────────────────────────
  id                     uuid  PK  ← NEW (was no PK)
  habit_id               uuid  FK → habits UNIQUE  ← CHANGED (was user_id)
  current_streak_seconds bigint           ← NEW (replaces CurrentStreakDays)
  last_server_utc_ms     bigint           ← NEW: last verified server timestamp (ms since epoch)
  last_sync_at           timestamptz      ← NEW: when server sync occurred
  updated_at             timestamptz
```

Old columns `current_streak_days`, `last_abstinent_day`, `last_non_abstinent_day`, `user_id` are dropped via migration.

---

### 3. `Relapse` — Add `PreviousStreakSeconds`, encryption flag

```
relapses table
─────────────────────────────────────────────
  id                       uuid  PK
  habit_id                 uuid  FK → habits
  user_id                  uuid  FK → users   (kept for fast user-level queries)
  occurred_at              timestamptz  (immutable after insert)
  context_note_encrypted   bytea        ← RENAMED from context_note (now encrypted bytes)
  previous_streak_seconds  bigint       ← NEW: streak duration at time of relapse
  created_at               timestamptz
```

---

### 4. `DayAbstinenceStatus` — Derived, not stored

Computed per-request from:
- `Habit.StartedAt`
- `Relapse` rows for the habit (by `OccurredAt` date in user's timezone)
- Archived periods (stub: empty for MVP, pending Feature 005)

Not persisted — recalculation is O(R) where R = relapse count for the period requested.

---

### 5. `PeriodCompliance` — Derived, not stored

For `HabitMode.Reduce` habits only. Computed from Relapse timestamps within a calendar week (Mon–Sun UTC boundary). Not persisted for MVP.

---

## Calculation Domain Model (Backend — `Domain/Calculation/`)

### `StreakCalculator` (static pure functions)

```
StreakCalculator
  + CalculateCurrentSeconds(startedAt: DateTimeOffset, relapses: IReadOnlyList<DateTimeOffset>, now: DateTimeOffset) → long
  + CalculateDayStatus(date: DateOnly, userTimeZone: TimeZoneInfo, startedAt: DateTimeOffset, relapses: IReadOnlyList<DateTimeOffset>) → DayStatus
  + CalculateMonthStats(year: int, month: int, userTimeZone: TimeZoneInfo, startedAt: DateTimeOffset, relapses: IReadOnlyList<DateTimeOffset>, today: DateOnly) → MonthStats
  + CalculateYearStats(year: int, userTimeZone: TimeZoneInfo, startedAt: DateTimeOffset, relapses: IReadOnlyList<DateTimeOffset>, today: DateOnly) → YearStats
  + CalculatePeriodCompliance(weekStart: DateOnly, relapses: IReadOnlyList<DateTimeOffset>, maxPerWeek: int) → PeriodComplianceStatus
```

### Value types

```
DayStatus: enum { Abstinent, Relapse, Paused, Neutral }

MonthStats {
  AbstinentDays: int
  RelevantDays: int      // denominator (today-capped for current month)
  RelapseCount: int
}

YearStats {
  Months: IReadOnlyList<MonthStats>   // index 0 = January
  TotalAbstinentDays: int
  TotalRelevantDays: int
}

PeriodComplianceStatus: enum { Compliant, Exceeded, Pending }
```

---

## Calculation Domain Model (Frontend — `src/lib/streak-calc/`)

```typescript
// streak-calc/index.ts — pure functions, no framework deps

calculateCurrentSeconds(startedAtUtcMs: number, relapses: number[], nowUtcMs: number): number
calculateDayStatus(dateIso: string, userTimezone: string, startedAtUtcMs: number, relapses: number[]): DayStatus
calculateMonthStats(year: number, month: number, userTimezone: string, startedAtUtcMs: number, relapses: number[], todayIso: string): MonthStats
calculateYearStats(year: number, userTimezone: string, startedAtUtcMs: number, relapses: number[], todayIso: string): YearStats
calculatePeriodCompliance(weekStartIso: string, relapses: number[], maxPerWeek: number): PeriodComplianceStatus
```

`relapses` = sorted array of UTC ms timestamps. All timezone math via `Intl.DateTimeFormat`.

---

## Monotonic Offset Store (Frontend — `src/lib/streak-calc/monotonic-store.ts`)

```typescript
interface MonotonicSnapshot {
  serverUtcMs: number        // verified server time
  performanceNowMs: number   // performance.now() at sync moment
  syncedAt: number           // Date.now() at sync moment (for stale detection)
}

saveSnapshot(snapshot: MonotonicSnapshot): void   // persists to localStorage
loadSnapshot(): MonotonicSnapshot | null
getNowUtcMs(): number   // returns serverUtcMs + (performance.now() - performanceNowMs)
```
