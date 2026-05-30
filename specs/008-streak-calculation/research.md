# Research: 008 — Streak- und Abstinenz-Berechnung

**Phase**: 0 — Discovery | **Date**: 2026-05-30 | **Branch**: `008-streak-calculation`

## Codebase Findings

### Backend (.NET 10, EF Core + PostgreSQL)

| Component | Current State | Gap vs. Spec 008 |
|---|---|---|
| `Streak` entity | Day-based (`CurrentStreakDays`, `LastAbstinentDay`); per-user, no HabitId | Must store UTC-seconds + server timestamp + monotonic offset; needs HabitId FK |
| `Habit.StartedOn` | `DateOnly` — day precision only | Must become `DateTimeOffset` (`StartedAt`) for second-accurate streak from FR-004 |
| `Relapse` entity | `OccurredAt` (DateTimeOffset), `ContextNote` (string) — no encryption marker, no `PreviousStreakSeconds` | Add `PreviousStreakSeconds long`, encryption column flag |
| `StreakService` | CheckIn-based day counting; no UTC-second precision, no Relapse-awareness | Full rewrite: UTC-second precision, Relapse-aware, monotonic delta, manipulation detection |
| `CheckIn` entity | Day-based (DateOnly) | Unchanged — not in scope for this feature |
| `QuitlyDbContext` | Has `Streaks`, `Relapses`, `Habits` DbSets | New migration required for schema changes |

### Frontend (React 19, Vite, Vitest, TanStack Query, PrimeReact)

| Component | Current State | Gap |
|---|---|---|
| `StreakCard` | Shows `currentStreakDays` (integer, no sub-day precision) | Needs seconds-accurate live counter (H:M:S display) |
| `features/streak/` | Only `StreakCard.tsx` + spec | New: CalendarView, MonthStats, YearHeatmap |
| Calculation module | None — server does all calc | Need shared UTC-math utilities in `src/lib/streak-calc/` |
| Property-based tests | None | `fast-check` (npm) for TypeScript PBT |
| Accessibility | No WCAG audit tooling in streak UI | `@axe-core/playwright` already in devDeps — use for calendar view |

### Encryption Infrastructure

`User` has `PasswordHash` (Argon2). No application-level encryption utility found for data-at-rest fields. Need to introduce `IDataProtector` (ASP.NET Core Data Protection) or a dedicated `FieldEncryptor` service for `Relapse.ContextNote` encryption. **Decision**: Use ASP.NET Core Data Protection (already in the platform) with a purpose string scoped per user.

### Shared Calculation Module Strategy

**Decision**: The "shared module" (FR-006) maps to:
- Backend: `StreakCalculator` static class in `Domain/` (pure functions, no DI, fully testable)  
- Frontend: `src/lib/streak-calc/` TypeScript module (same pure-function interface)
- Both implement identical algorithm; property-based tests run on both independently
- No code sharing mechanism (no monorepo/WASM) — specification requires identical *behavior*, not identical *code*

### Monotonic Clock (Frontend)

`performance.now()` gives a monotonic timestamp in milliseconds relative to navigation start. Persist last verified server UTC timestamp in localStorage; offline delta = `performance.now() - lastSyncPerformanceNow` added to `lastServerUtcMs`. This avoids `Date.now()` which follows wall-clock changes.

### Timezone Handling

Use the `Intl.DateTimeFormat` API (no library) for converting UTC boundaries to local calendar dates. All day boundaries computed in UTC; display formatted with user's `Intl.DateTimeFormat` locale. No `Temporal` API usage — insufficient browser support as of 2026-05.

### Property-based Test Libraries

- **C#**: `FsCheck.Xunit` (stable, widely used) — add as test-only dependency
- **TypeScript**: `fast-check` (v3, ESM-compatible with Vitest) — add as devDependency

### Interval/Period Habits

`HabitMode.Reduce` maps to the interval/period habit concept (FR-012). `PeriodCompliance` is a derived/computed view — not stored as a table; calculated on-demand from `Relapse` events within the period window.

### Calendar Performance

Calendar month view requires: habit start date, all Relapse timestamps for the month, all archived periods (Feature 005 — not yet built, stub as "no pause periods" for now). Day status derivation is O(n) per relapse — acceptable for typical usage (< 50 relapses/month).

## Open Questions (Resolved as Assumptions)

| Question | Resolution |
|---|---|
| EF Core migration for Habit.StartedOn DateOnly → DateTimeOffset | Breaking schema change; migration must preserve existing data (convert DateOnly to midnight UTC DateTimeOffset) |
| Streak.UserId → Streak.HabitId FK | Streak becomes per-habit, not per-user. Existing single-habit assumption means migration drops the old user-level streak row |
| PeriodCompliance storage | Derived/computed on-demand; no dedicated table for MVP |
| Archived periods (pause status) | Feature 005 not yet implemented; DayStatus "pausiert" stubbed as "never" for now — noted in plan scope |
