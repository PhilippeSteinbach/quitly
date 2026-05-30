# Quickstart: 008 — Streak- und Abstinenz-Berechnung

**Phase**: 1 | **Date**: 2026-05-30 | **Branch**: `008-streak-calculation`

## Prerequisites

- .NET 10 SDK, Node 22+, PostgreSQL running locally (or Docker)
- `cd c:\Dev\quitly`

---

## Backend: Run & Migrate

```powershell
# Start backend (from repo root)
cd backend
dotnet run --project src/Quitly.Api.csproj

# Apply new migrations after entity changes
dotnet ef migrations add AddStreakCalculation --project src/Quitly.Api.csproj
dotnet ef database update --project src/Quitly.Api.csproj
```

### Add FsCheck.Xunit (property-based tests)

```powershell
cd backend
dotnet add tests/ package FsCheck.Xunit
```

### Run backend tests

```powershell
cd backend
dotnet test
```

---

## Frontend: Run & Test

```powershell
cd frontend
npm install          # installs existing deps
npm run dev          # starts Vite dev server

# Add fast-check for property-based tests
npm install --save-dev fast-check

# Run unit tests
npm test

# Run accessibility checks (Playwright)
npx playwright test
```

---

## Key Implementation Entry Points

| What | Where |
|---|---|
| Streak pure calculator (backend) | `backend/src/Domain/Calculation/StreakCalculator.cs` ← **create** |
| Streak pure calculator (frontend) | `frontend/src/lib/streak-calc/index.ts` ← **create** |
| Monotonic store (frontend) | `frontend/src/lib/streak-calc/monotonic-store.ts` ← **create** |
| Streak service (backend) | `backend/src/Application/Streaks/StreakService.cs` ← **rewrite** |
| New calendar service (backend) | `backend/src/Application/Streaks/CalendarService.cs` ← **create** |
| New stats service (backend) | `backend/src/Application/Streaks/StatsService.cs` ← **create** |
| Relapse encryption service | `backend/src/Infrastructure/Security/FieldEncryptor.cs` ← **create** |
| New API endpoints | `backend/src/Api/Streaks/StreakEndpoints.cs` ← **create** |
| StreakCard (frontend) | `frontend/src/features/streak/StreakCard.tsx` ← **update** (seconds counter) |
| CalendarView (frontend) | `frontend/src/features/streak/CalendarView.tsx` ← **create** |
| MonthStats (frontend) | `frontend/src/features/streak/MonthStats.tsx` ← **create** |
| YearHeatmap (frontend) | `frontend/src/features/streak/YearHeatmap.tsx` ← **create** |
| Streak API client (frontend) | `frontend/src/services/streak.api.ts` ← **create** |

---

## EF Core Migration Notes

The migration must:
1. Add `started_at timestamptz` to `habits` — populate from `started_on AT TIME ZONE 'UTC'`
2. Drop `streaks.current_streak_days`, `streaks.last_abstinent_day`, `streaks.last_non_abstinent_day`, `streaks.user_id`
3. Add `streaks.id uuid`, `streaks.habit_id uuid`, `streaks.current_streak_seconds bigint`, `streaks.last_server_utc_ms bigint`, `streaks.last_sync_at timestamptz`
4. Add `relapses.previous_streak_seconds bigint`
5. Rename `relapses.context_note` → `relapses.context_note_encrypted` (type change: text → bytea)

---

## Verification Checklist (dev)

- [ ] `GET /habits/{id}/streak` returns `currentStreakSeconds` and `serverUtcMs`
- [ ] `GET /habits/{id}/calendar/2026/5` returns 31 days with correct statuses
- [ ] `GET /habits/{id}/stats/2026/5` returns correct `relevantDays` (today-capped)
- [ ] `GET /time` returns server UTC ms without auth
- [ ] StreakCard shows live H:M:S counter updating every second
- [ ] CalendarView renders green/red/grey day cells with correct statuses
- [ ] Tap on relapse day shows all relapses chronologically
- [ ] YearHeatmap shows 12 month tiles; months before start are neutral/empty
- [ ] Property-based tests pass for timezone, leap year, DST scenarios
- [ ] Playwright accessibility scan passes on CalendarView (WCAG 2.2 AA)
