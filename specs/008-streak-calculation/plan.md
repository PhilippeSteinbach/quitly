# Implementation Plan: Präzise Streak- und Abstinenz-Berechnung inkl. Rückfall-Logik

**Branch**: `008-streak-calculation` | **Date**: 2026-05-30 | **Spec**: [spec.md](spec.md)

## Summary

Dieses Feature ersetzt die aktuelle Tag-basierte Streak-Berechnung durch eine sekundengenaue, manipulationssichere, UTC-korrekte Berechnung. Es fügt eine Kalenderansicht und Monats-/Jahresstatistiken hinzu. Die Kernlogik (Streak, Tages-Status, Statistik-Aggregation) wird in zwei identisch verhaltenden reinen Berechnungsmodulen implementiert — eines im Backend (C#), eines im Frontend (TypeScript) — und durch Property-based Tests gegen Zeitzonen, Schaltjahre und DST abgesichert.

## Technical Context

**Backend**: .NET 10 Minimal API, EF Core + PostgreSQL (Npgsql), ASP.NET Core Data Protection (für Feldverschlüsselung)

**Frontend**: React 19, TypeScript, Vite, Vitest, TanStack Query, PrimeReact, Tailwind CSS

**Testing**: Vitest + `fast-check` (PBT Frontend), xUnit + `FsCheck.Xunit` (PBT Backend), `@axe-core/playwright` (Accessibility)

**Target Platform**: Web (React frontend) + .NET 10 API server

**Version Policy**: Neueste stabile Versionen zum Planungsdatum; abweichende Pins mit Begründung in quickstart.md

**Performance Goals**: P95 Kalenderansicht (Monat) ≤ 300 ms; Streak-Anzeige ± 1 s Abweichung zur Serverzeit

**Constraints**: Offline-fähig (monotone Uhr + localStorage); WCAG 2.2 AA für Kalenderansicht

## Constitution Check

| Gate | Status | Evidence |
|---|---|---|
| **User Welfare** | ✅ | Statistiken zeigen "X von Y Tagen abstinent" — Fokus auf Erreichten; kein Streak-Druck-Mechanismus |
| **Non-shaming Relapse** | ✅ | Manipulations-Korrektur: sachlicher Toast "Deine Streak wurde mit dem Server abgeglichen"; Rückfall-Kalender ohne Wertung |
| **Privacy** | ✅ | `ContextNote` at-rest verschlüsselt (ASP.NET Core Data Protection, account-bound); `/time` gibt keine Nutzerdaten heraus; Minimum-Daten-Prinzip |
| **UX Clarity** | ✅ | Primärpfad: Dashboard → Streak-Counter → Kalender (optional) → Statistik (optional). Jede Ansicht hat eine Kernaussage |
| **Quality Gates** | ✅ | PBT deckt Zeitzonen/Schaltjahr/DST ab; WCAG 2.2 AA via axe-core/playwright; P95 ≤ 300 ms; Reliability ≥ 99 % |
| **Scope** | ✅ | MVP. Kalender + Statistik sind direkte Ausgabe der Berechnungslogik — zusammen Mindestvisualisierung |
| **Conflict Resolution** | ✅ | Genauigkeit vs. Offline: Offline-Verfügbarkeit priorisiert; Manipulation: nur negative Abweichung, Constitution II Vorrang |
| **DoD** | ✅ | PBT + Axe + P95-Messung als Freigabekriterien definiert |

## Project Structure

### Documentation (this feature)

```text
specs/008-streak-calculation/
├── plan.md              ← this file
├── research.md          ← Phase 0 findings
├── data-model.md        ← entity changes + calculation domain model
├── quickstart.md        ← dev setup and entry points
├── contracts/
│   └── api.md           ← REST API contracts + TypeScript DTOs
└── checklists/
    └── requirements.md  ← quality checklist (16/16)
```

### Source Code

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Calculation/
│   │   │   └── StreakCalculator.cs         ← NEW: pure static calculation functions
│   │   └── Entities/
│   │       ├── Habit.cs                    ← UPDATE: add StartedAt (DateTimeOffset)
│   │       ├── Streak.cs                   ← UPDATE: per-habit, UTC-seconds schema
│   │       └── Relapse.cs                  ← UPDATE: add PreviousStreakSeconds, rename ContextNote
│   ├── Application/
│   │   └── Streaks/
│   │       ├── StreakService.cs             ← REWRITE: UTC-seconds, relapse-aware, manipulation detection
│   │       ├── CalendarService.cs          ← NEW
│   │       └── StatsService.cs             ← NEW
│   ├── Infrastructure/
│   │   └── Security/
│   │       └── FieldEncryptor.cs           ← NEW: ASP.NET Core Data Protection wrapper
│   ├── Api/
│   │   └── Streaks/
│   │       └── StreakEndpoints.cs          ← NEW: 6 endpoints
│   └── Persistence/
│       ├── QuitlyDbContext.cs              ← UPDATE
│       └── Migrations/                     ← NEW migration: AddStreakCalculation
└── tests/
    └── Streaks/
        ├── StreakCalculatorTests.cs         ← NEW: unit tests
        └── StreakCalculatorPropertyTests.cs ← NEW: FsCheck property-based tests

frontend/
├── src/
│   ├── lib/
│   │   └── streak-calc/
│   │       ├── index.ts                    ← NEW: pure calculation functions
│   │       ├── monotonic-store.ts          ← NEW: localStorage monotonic offset store
│   │       └── index.spec.ts               ← NEW: Vitest unit + fast-check PBT
│   ├── services/
│   │   └── streak.api.ts                   ← NEW: TanStack Query hooks + DTOs
│   └── features/
│       └── streak/
│           ├── StreakCard.tsx              ← UPDATE: live seconds counter
│           ├── CalendarView.tsx            ← NEW
│           ├── CalendarView.spec.tsx       ← NEW: unit + axe accessibility test
│           ├── MonthStats.tsx              ← NEW
│           ├── YearHeatmap.tsx             ← NEW
│           └── YearHeatmap.spec.tsx        ← NEW
└── tests/
    └── streak-a11y.spec.ts                 ← NEW: Playwright + axe-core WCAG 2.2 AA
```

**Structure Decision**: Option 2 (Web application — separate `backend/` and `frontend/` trees). Existing project structure confirmed by codebase inspection.

## Implementation Phases

### Phase A — Backend: Domain Layer *(blocker for all other backend phases)*

**Goal**: Pure, fully-tested calculation engine. No side effects, no DI.

1. Create `backend/src/Domain/Calculation/StreakCalculator.cs`
   - `CalculateCurrentSeconds(startedAt, relapses, now)` — seconds since last relapse (or startedAt)
   - `CalculateDayStatus(date, tz, startedAt, relapses)` — returns `DayStatus` enum
   - `CalculateMonthStats(year, month, tz, startedAt, relapses, today)` — returns `MonthStats` with today-capped denominator for current month
   - `CalculateYearStats(year, tz, startedAt, relapses, today)` — 12 `MonthStats`
   - `DetectManipulation(offlineDeltaMs, serverDeltaMs)` — true only for negative deviation > 300 000 ms
2. Write `StreakCalculatorTests.cs` — unit tests for each function, all edge cases (midnight UTC, DST boundary, leap Feb 29, relapse on start day, multiple relapses same day)
3. Write `StreakCalculatorPropertyTests.cs` using FsCheck:
   - `CalculateCurrentSeconds` never returns negative
   - Timezone shift does not change total seconds
   - Adding a relapse at T makes `CalculateCurrentSeconds(T+1s)` < `CalculateCurrentSeconds(T-1s)`
   - `MonthStats.RelevantDays` ≤ days-in-month for all valid inputs

**Acceptance**: `dotnet test` green including property tests.

---

### Phase B — Backend: Schema Migration

**Goal**: DB schema aligned with data-model.md.

1. `Habit.cs` — add `StartedAt DateTimeOffset?`
2. `Streak.cs` — replace all columns with per-habit schema (id, habit_id FK UNIQUE, current_streak_seconds, last_server_utc_ms, last_sync_at)
3. `Relapse.cs` — add `PreviousStreakSeconds long`, change `ContextNote string?` → `ContextNoteEncrypted byte[]?`
4. `QuitlyDbContext.cs` — configure `bytea` column type for `ContextNoteEncrypted`, update Streak entity config
5. EF migration `AddStreakCalculation`: populate `started_at` from `started_on AT TIME ZONE 'UTC'`; drop old streak columns; add new streak columns
6. `dotnet ef database update` locally

**Acceptance**: Migration runs cleanly on fresh and existing DB; no data loss.

---

### Phase C — Backend: Infrastructure & Services

**Goal**: Encryption and application services.

1. Create `FieldEncryptor.cs` using `IDataProtectionProvider` — `Encrypt(plaintext, userId)` / `Decrypt(bytes, userId)` with user-scoped purpose string
2. Rewrite `StreakService.cs`:
   - `GetStreakAsync(habitId)` — loads habit + relapse timestamps, calls `StreakCalculator.CalculateCurrentSeconds`, persists updated `Streak`, returns `StreakDto` with `serverUtcMs`
   - `SyncAndCorrectAsync(habitId, offlineDeltaMs)` — calls `DetectManipulation`; if true, corrects streak and sets correction flag
3. Create `CalendarService.cs` — loads relapse timestamps for month, calls `CalculateDayStatus` per day, decrypts notes via `FieldEncryptor`
4. Create `StatsService.cs` — calls `CalculateMonthStats` / `CalculateYearStats`
5. Register services in `Program.cs`

**Acceptance**: Unit tests with mocked DbContext pass; encryption round-trips correctly.

---

### Phase D — Backend: API Endpoints

**Goal**: All 6 endpoints live per contracts/api.md.

1. Create `StreakEndpoints.cs` with `MapStreakEndpoints(IEndpointRouteBuilder app)`:
   - `GET /habits/{habitId}/streak`
   - `GET /habits/{habitId}/calendar/{year}/{month}`
   - `GET /habits/{habitId}/stats/{year}/{month}`
   - `GET /habits/{habitId}/stats/{year}`
   - `POST /habits/{habitId}/relapses` (validate occurredAt ≤ now; ≥ habit.StartedAt; contextNote ≤ 500 chars)
   - `GET /time` (public, no auth; rate-limit 60/min per IP)
2. Register endpoint group in `Program.cs`

**Acceptance**: Integration tests confirm all endpoints return correct response shapes and error codes.

---

### Phase E — Frontend: Calculation Module *(parallel to Phases A–D)*

**Goal**: TypeScript calculation module with identical behaviour to C# counterpart.

1. Create `src/lib/streak-calc/index.ts` — 5 pure functions using `Intl.DateTimeFormat` for timezone math; zero external dependencies
2. Create `src/lib/streak-calc/monotonic-store.ts` — `saveSnapshot`, `loadSnapshot`, `getNowUtcMs`; stale guard (> 24 h falls back to `Date.now()`)
3. Write `index.spec.ts` using Vitest + `fast-check`:
   - Same 4 properties as backend
   - Explicit: Feb 29 2028, DST spring-forward 2026-03-29 Europe/Berlin, DST fall-back 2026-10-25
   - MonthStats denominator today-capped for current month

**Acceptance**: `npm test` green including property-based tests.

---

### Phase F — Frontend: API Integration

**Goal**: TanStack Query hooks for all endpoints.

1. Create `src/services/streak.api.ts` with hooks: `useStreak`, `useCalendarMonth`, `useMonthStats`, `useYearStats`, `useRecordRelapse`
2. `useStreak` success handler: call `saveSnapshot({ serverUtcMs, performanceNowMs: performance.now() })`
3. Visibility-change listener: re-fetch streak on foreground; if manipulation detected (server correction flag), show toast

**Acceptance**: Hooks typed correctly; manipulation toast shown in Vitest integration test.

---

### Phase G — Frontend: UI Components

**Goal**: All views rendered correctly with WCAG 2.2 AA.

1. **`StreakCard.tsx`** — `currentStreakSeconds` + `getNowUtcMs()` via `setInterval(1000)`; display "X Tage Y Std Z Min"
2. **`CalendarView.tsx`** — month grid; cells coloured by status; second WCAG marker (pattern/symbol per status); tap → sheet with chronological relapse list (all relapses, scrollable)
3. **`CalendarView.spec.tsx`** — unit tests for cell colouring + axe accessibility assertion
4. **`MonthStats.tsx`** — "X von Y Tagen abstinent"; "laufender Monat" badge if `isCurrentMonth`
5. **`YearHeatmap.tsx`** — 12-tile grid; neutral empty tiles for months before `startedAt`; accessible title per tile
6. **`YearHeatmap.spec.tsx`** — unit tests for neutral tile logic

**Acceptance**: All specs green; no axe violations on CalendarView.

---

### Phase H — Accessibility & End-to-End

**Goal**: WCAG 2.2 AA verified; P95 latency measured.

1. Write `tests/streak-a11y.spec.ts` (Playwright + `@axe-core/playwright`):
   - Load CalendarView with seeded data; run `checkA11y()`; assert 0 violations
   - Assert second visual marker present for each day status
   - Assert accessible labels on calendar tiles
2. Measure P95 response time for calendar endpoint under 50 concurrent requests — verify ≤ 300 ms
3. Verify `GET /time` rate-limit (> 60 req/min → 429)

**Acceptance**: 0 axe violations; P95 ≤ 300 ms; rate-limit active.

---

## Complexity Tracking

> No Constitution violations to justify.

---

## Dependency Order

```
Phase A (Backend Domain) ──┐
Phase E (Frontend Calc)  ──┤── independent, can be parallel
                           │
Phase A → Phase B (Schema) → Phase C (Services) → Phase D (Endpoints)
Phase E → Phase F (API hooks) → Phase G (UI)
Phase D + Phase G → Phase H (A11y + E2E)
```

---

## Risks & Mitigations

| Risk | Likelihood | Mitigation |
|---|---|---|
| EF Core migration data loss on `context_note` → `context_note_encrypted` (bytea) | Medium | Run Encrypt() in migration data step; test on DB copy before prod apply |
| `Intl.DateTimeFormat` DST edge case in Vitest (jsdom) | Medium | Pin TZ env var in test runner; use real browser in Playwright for acceptance |
| Monotonic offset stale after long background session | Low | `syncedAt` stale guard: > 24 h → fall back to `Date.now()` + immediate re-sync |
| Feature 005 (pause/archive) not yet implemented | Known | `DayStatus.Paused` stubbed as never for MVP; revisit when Feature 005 is planned |

---

## Post-MVP Scope (excluded)

- React Native / Expo calendar view (mobile)
- Shared WASM or npm package for true cross-platform calculation code
- Offline relapse recording with sync queue (current plan: POST /relapses requires online connection)
