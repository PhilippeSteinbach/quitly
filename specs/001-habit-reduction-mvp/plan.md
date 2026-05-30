# Implementation Plan: Quitly Habit Reduction MVP

**Branch**: `002-before-specify-hook` | **Date**: 2026-05-28 | **Spec**: `/specs/001-habit-reduction-mvp/spec.md`

**Input**: Feature specification from `/specs/001-habit-reduction-mvp/spec.md`

## Summary

Quitly MVP liefert einen schnellen, responsiven Kern fuer nachhaltige Verhaltensaenderung:
Onboarding, taeglicher Check-in, strikte Streak-Logik, minimaler nicht-beschaemender
Relapse-Recovery-Flow, passive In-App-Hinweise und Wochen-Insights. Die Architektur ist
als klar getrennte Web-App mit React-Frontend und .NET Minimal API Backend ausgelegt,
datensparsam (keine Analytics-Events im MVP), wartbar und gezielt erweiterbar fuer
Post-MVP-Funktionen wie Craving-Interventionen und Achievements.

## Technical Context

**Language/Version**:
- Frontend: TypeScript 5.x (latest stable), React 19.x (latest stable)
- Backend: C# 14 / .NET 10 (Minimal API)

**Primary Dependencies**:
- Frontend: React, shadcn/ui, Tailwind CSS, TanStack Query, Axios
- Backend: ASP.NET Core Minimal API, EF Core 10, Npgsql provider
- Shared: OpenAPI 3.1 contract-first API documentation

**Storage**:
- PostgreSQL 18 (primary relational store)

**Testing**:
- Frontend: Vitest + React Testing Library + Playwright (critical flows)
- Backend: xUnit + FluentAssertions + Testcontainers (PostgreSQL)
- Contract: OpenAPI schema validation and API contract tests

**Target Platform**:
- Responsive web (mobile-first + desktop)
- Linux container deployment target (staging/prod)

**Project Type**:
- Web application (frontend + backend)

**Performance Goals**:
- P95 API response for core read/write endpoints <= 300 ms
- P95 first meaningful dashboard render <= 2.0 s on mid-tier mobile network
- P95 check-in submit interaction <= 1.5 s end-to-end

**Constraints**:
- DSGVO-sensitive, strict data minimization, no analytics events in MVP
- Rapid delivery: 3 iterative release milestones
- Maintainability: modular bounded contexts, migration-safe persistence model
- Accessibility target: WCAG 2.2 AA

**Scale/Scope**:
- MVP: single primary habit per user, core daily journey, insights
- Early scale assumption: 5k-20k MAU, peak 30 req/s, data retention policy configurable

## Proposed Architecture

### High-Level

- Frontend SPA (React) consumes backend Minimal API via HTTPS JSON.
- Backend uses application services + repository-style data access over EF Core.
- PostgreSQL stores user-owned records with strict ownership boundaries.
- Background scheduler (internal hosted service) computes weekly insights.
- API contract versioned under `/api/v1`.

### Module Boundaries

- Identity & Access: user auth/session and ownership checks
- Habit Setup: onboarding and goal definition
- Daily Reflection: check-ins, triggers, streak calculations
- Relapse Recovery: minimal non-shaming relapse mark and 24h next-step capture
- Guidance: passive in-app prompts
- Insights: weekly summaries and trend interpretation
- Post-MVP placeholders: craving interventions, achievements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Gate Assessment

- User welfare gate: PASS. No addictive engagement mechanics; passive hints only.
- Non-shaming relapse gate: CONDITIONAL. Passes after MVP includes minimal relapse
  recovery flow with non-punitive language and explicit relapse-path tests.
- Privacy gate: PASS. No analytics events in MVP; only product-essential data.
- UX clarity gate: PASS. One primary action per screen, short daily interaction path.
- Quality gate: PASS. Explicit reliability/accessibility/performance budgets defined.
- Scope gate: PASS. MVP and Post-MVP boundaries explicitly separated.
- Conflict gate: PASS. Prioritization order from constitution captured in decision rules.
- DoD gate: PASS. Evidence expectations defined for testing, privacy, rollout, rollback.

### Post-Phase 1 Re-Check

- PASS. Data model, contracts, quickstart and risk controls remain constitution-aligned.
- No unjustified constitutional violations identified.

## API Interfaces (Design Overview)

- `POST /api/v1/auth/register` and `POST /api/v1/auth/login` for identity bootstrap.
- `GET /api/v1/habit` and `PUT /api/v1/habit` for single active habit goal.
- `POST /api/v1/check-ins` and `GET /api/v1/check-ins` for daily reflection.
- `GET /api/v1/streak` for computed current streak and trend metadata.
- `GET /api/v1/prompts/today` and `PUT /api/v1/prompts/preferences` for passive hints.
- `POST /api/v1/relapse` and `POST /api/v1/recovery-steps` for minimal MVP recovery.
- `GET /api/v1/insights/weekly` for weekly trend summaries.
- Post-MVP reserved endpoints for interventions and achievements.

## Auth and Security

- Authentication: email/password with Argon2id password hashing.
- Session model: short-lived JWT access token + rotating refresh token.
- Authorization: strict resource ownership checks at API boundary.
- Transport: TLS enforced, HSTS in production.
- Data protection: encryption at rest via platform-managed storage encryption.
- Secrets: environment-bound secret manager (no static secrets in repo).
- Abuse protection: rate limiting on auth and write-heavy endpoints.

## Analytics Plan (MVP-Compatible)

- No analytics tracking events in MVP by constitutional and clarified scope decision.
- KPI computation from product-operational tables only (check-ins, prompts, insights use).
- Aggregation jobs produce non-personalized product metrics for milestone evaluation.
- Post-MVP decision point: explicit consented analytics model, if needed.

## Test Strategy

- Unit tests:
  - Frontend view logic, form validation, accessibility helpers
  - Backend domain rules (streak, prompt eligibility, insight aggregation)
- Integration tests:
  - API + PostgreSQL via Testcontainers
  - Auth flow, habit setup, check-in lifecycle, relapse recovery, weekly insight retrieval
- Contract tests:
  - Validate implementation against OpenAPI schema
- E2E tests:
  - Onboarding -> check-in -> relapse recovery -> streak view -> weekly insights
- Non-functional checks:
  - Lighthouse CI budgets, basic load smoke tests, accessibility scans (axe)

## Release Milestones

1. Milestone 1: Foundation and Identity
   - Repo structure, CI, auth baseline, user/habit persistence, initial UI shell
2. Milestone 2: MVP Core Journey
   - Onboarding, daily check-in flow, streak endpoint/UI, minimal recovery flow,
     passive prompts
3. Milestone 3: Weekly Insights and Hardening
   - Insight generation (including recovery-continuity signals), KPI aggregation from
     operational data, accessibility/perf pass, release checklist and rollout plan

## Risks and Mitigations

- Risk: Scope drift from Post-MVP entities into MVP implementation.
  - Mitigation: enforce scope labels in tasks and PR checks.
- Risk: Streak strictness may negatively affect user motivation.
  - Mitigation: neutral UX copy and trend framing around consistency, not blame.
- Risk: Recovery flow may bloat MVP scope and delay ship date.
  - Mitigation: enforce minimal recovery slice (mark relapse, capture one next step,
    confirm continuation) and defer richer interventions.
- Risk: Weekly insight quality may be low with sparse data.
  - Mitigation: confidence labels and fallback insight text for low-signal weeks.
- Risk: Security regression in token lifecycle.
  - Mitigation: dedicated integration tests for refresh rotation and token revocation.

## Open Decisions

- Decide auth provider approach: fully in-house vs managed identity service.
- Decide retention windows for check-ins and derived insights under DSGVO policy.
- Decide whether achievements remain Post-MVP or become MVP-lite read-only badges.
- Decide reminder preference depth in MVP (on/off only vs lightweight timing window).

## Version Currency Policy

- Default rule: always use the latest stable major/minor versions available at planning
  date for framework and platform choices.
- If a non-latest version is selected, the plan MUST include explicit risk/compatibility
  rationale and an upgrade milestone.

## Project Structure

### Documentation (this feature)

```text
specs/001-habit-reduction-mvp/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── quitly-api.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── api/
│   ├── application/
│   ├── domain/
│   ├── infrastructure/
│   └── persistence/
└── tests/
    ├── unit/
    ├── integration/
    └── contract/

frontend/
├── src/
│   ├── app/
│   ├── components/
│   ├── features/
│   ├── lib/
│   └── services/
└── tests/
    ├── unit/
    ├── integration/
    └── e2e/
```

**Structure Decision**: Option "Web application" selected to preserve clear separation
between UI delivery and domain/API evolution while keeping fast MVP iteration.

## Complexity Tracking

No constitutional violations requiring justification.
