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

## 6. Release Readiness
1. Verify constitutional DoD checklist evidence.
2. Prepare rollout and rollback notes.
3. Tag milestone release candidate.
