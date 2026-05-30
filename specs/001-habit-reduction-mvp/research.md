# Research: Quitly Habit Reduction MVP

## Decision 1: Split Web Architecture (React SPA + .NET Minimal API)
- Decision: Use separate frontend and backend projects with a contract-first API boundary.
- Rationale: Enables fast frontend iteration with independent backend hardening and clear team ownership.
- Alternatives considered:
  - Full-stack monolith with server-rendered UI: simpler deploy, but weaker UI iteration velocity.
  - Backend-for-frontend dedicated layer: unnecessary complexity for MVP scale.

## Decision 2: Data Layer with EF Core + PostgreSQL
- Decision: Persist domain data in PostgreSQL using EF Core migrations.
- Rationale: Strong relational integrity for user-owned records and simple schema evolution.
- Alternatives considered:
  - Document database: weaker fit for relational constraints and reporting joins.
  - Raw SQL only: less maintainable for MVP team throughput.

## Decision 3: MVP Privacy Model without Analytics Events
- Decision: No analytics tracking events in MVP; KPI derivation from operational product data.
- Rationale: Aligns with constitution and clarified scope, minimizes privacy risk and consent overhead.
- Alternatives considered:
  - Event analytics with consent: adds legal and UX complexity before core value is proven.
  - Anonymous analytics only: still introduces governance and data-flow complexity.

## Decision 4: Strict Streak Semantics
- Decision: Streak counts only fully abstinent days and resets on non-abstinent day.
- Rationale: Explicit and deterministic behavior for users and implementation.
- Alternatives considered:
  - Grace-day streak model: less transparent semantics.
  - Weighted consistency score only: less intuitive for MVP users.

## Decision 5: Scope Boundaries for MVP vs Post-MVP
- Decision: MVP includes onboarding, check-ins, strict streak, minimal relapse recovery, passive prompts, and weekly insights. Interventions and achievements remain feature-gated as Post-MVP.
- Rationale: Satisfies constitutional relapse-safety while preserving fast, testable delivery.
- Alternatives considered:
  - Move all relapse/intervention work to Post-MVP: faster initial scope but constitution conflict risk.
  - Expand full intervention suite into MVP: broader value but higher timeline and quality risk.

## Decision 6: Achievement Scope Activation (Post-MVP)
- Status: Pending
- Owner: Product Lead (with input from UX Lead and Engineering Lead)
- Decision deadline: End of Sprint 3 (prior to MVP public release)
- Decision: Defer the achievements/badges surface entirely until a dedicated activation decision is made. No achievement UI, persistence, or backend endpoint ships with the MVP. The frontend mounts the `PostMvpPlaceholder` route only when `VITE_FEATURE_POSTMVP=true`; the backend exposes no achievement schema or controller.
- Rationale: Achievement loops can incentivize relapses being under-reported (users protect a badge over reporting honestly), which conflicts with the constitutional relapse-safety principle. Activation requires explicit research that the chosen loop does not degrade honest self-report.
- Acceptance criteria for activation:
  1. Written research note (this file, Decision 6) flipped to `Status: Approved` with date and owner signature.
  2. Behavioral risk review confirming the chosen achievement loop does not penalize relapse honesty (e.g., no streak-loss-only badges).
  3. Telemetry plan that tracks honest-report rate before/after rollout.
  4. Feature flag `VITE_FEATURE_POSTMVP` removed in favor of a dedicated `VITE_FEATURE_ACHIEVEMENTS` flag, or replaced by server-driven entitlement.
- Trigger conditions for re-evaluation:
  - Two consecutive weekly insights cycles show stable honest-report metrics post-MVP.
  - User research surfaces a concrete demand for recognition that cannot be met by streaks + weekly insights alone.
- Alternatives considered:
  - Ship a minimal "first week" badge in MVP: rejected — too small to validate honesty risk, still requires schema/UI cost.
  - Permanent removal: rejected — premature; team wants the option once behavioral safeguards are designed.

## Decision 6: Security Baseline
- Decision: JWT access + rotating refresh tokens, resource ownership checks, auth endpoint rate limiting.
- Rationale: Reasonable security posture for MVP with low operational overhead.
- Alternatives considered:
  - Session-only server store: simpler revocation but harder horizontal scaling.
  - Third-party auth immediately: faster setup but external coupling introduced early.
