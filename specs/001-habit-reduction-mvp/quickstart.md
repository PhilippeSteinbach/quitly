# Quickstart: Quitly MVP Delivery

## Prerequisites
- .NET 10 SDK
- Node.js latest LTS (currently 24.x)
- PostgreSQL 18

## 1. Start Infrastructure
1. Create database `quitly` in local PostgreSQL.
2. Set environment variables:
   - `ConnectionStrings__Default`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__SigningKey`

## 2. Backend
1. Create backend solution under `backend/` using ASP.NET Core Minimal API.
2. Add packages: EF Core, Npgsql, auth packages, OpenAPI.
3. Implement initial migrations for User, Habit, CheckIn, Trigger, Streak, Relapse, RecoveryPlanStep, Reminder, WeeklyInsight.
4. Run migrations and start API.

## 3. Frontend
1. Bootstrap React + TypeScript app under `frontend/`.
2. Install Tailwind, shadcn/ui, TanStack Query, Axios.
3. Implement pages:
   - Onboarding
   - Daily Check-In
   - Dashboard (streak + passive prompt)
   - Weekly Insights
4. Wire API client and query cache invalidation.

## 4. Contract Validation
1. Ensure backend OpenAPI matches `contracts/quitly-api.openapi.yaml`.
2. Run contract tests for key endpoints.

## 5. Test and Quality Gates
1. Run unit/integration/contract tests.
2. Run accessibility checks (axe + keyboard navigation pass).
3. Run performance smoke tests for check-in and insights flows.
4. Confirm reliability target and privacy constraints (no analytics events in MVP).
 5. Verify security headers and rate limiting for auth and write-heavy routes.

## 6. Release Readiness
1. Verify constitutional DoD checklist evidence.
 2. Attach evidence for onboarding, check-in, relapse recovery, and weekly insights paths.
 3. Prepare rollout and rollback notes.
 4. Rollout notes: enable staging first, verify JWT config, DB connectivity, and passive prompt defaults.
 5. Rollback notes: redeploy previous image, restore previous environment variables, and disable newly introduced routes if DB migration state is uncertain.
 6. Tag milestone release candidate.

 ## 7. Release Checklist Evidence
 - Accessibility: skip link, visible labels, keyboard navigation spot-checks, and automated axe pass.
 - Performance: frontend performance smoke thresholds for check-in and insight routes.
 - Security: rate limiting active, HSTS in production, and secure response headers present.
 - Recovery continuity: relapse and 24h recovery-step tests executed.

 ## 8. Validation Run (2026-05-30)
 - Backend build: `dotnet build Quitly.slnx` → 0 warnings, 0 errors.
 - Backend tests: unit 5/5, contract 4/4, integration 1/1 passed.
 - EF migration: `InitialCreate` generated at `backend/src/Persistence/Migrations/`.
 - Frontend build: `npm run build` → vite bundle produced (`dist/index-*.js` ~388 kB).
 - Frontend tests: vitest 8/8 passed (component, integration, performance smoke, privacy gate).
 - Live Postgres: `dotnet ef database update` applied `InitialCreate` against `postgres:18-alpine` Docker container (11 snake_case tables + migration history).
 - Playwright E2E: chromium project 2/2 passed against `vite preview` on `http://localhost:4173` (Node 24 LTS).
 - Open follow-up: axe automation run against deployed UI.
