# Tasks: Quitly Habit Reduction MVP

**Input**: Design documents from `/specs/001-habit-reduction-mvp/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Reliability, accessibility, performance, privacy, contract, integration, and E2E tests are REQUIRED for story completion.

**Organization**: Tasks are grouped by Epic -> Feature -> User Story phases to allow independent implementation and validation.

## Epic -> Feature Map

- **Epic E1 Platform Foundation**
  - **F1**: Repo and delivery baseline
  - **F2**: Identity, security, and persistence foundation
- **Epic E2 MVP Core Value Loop**
  - **F3**: Onboarding and single active habit goal (US1)
  - **F4**: Daily check-in and strict streak engine (US2)
  - **F5**: Minimal relapse recovery flow (US3)
  - **F6**: Passive prompts and weekly insights (US5)
- **Epic E3 Quality and Release**
  - **F7**: Quality gates, observability, release readiness
- **Epic E4 Post-MVP Extension Tracks**
  - **F8**: Craving intervention scaffolding (US4)
  - **F9**: Achievement decision and optional rollout track

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Parallelizable (different files, no incomplete dependency)
- **[Story]**: User story label (US1, US2, US3, US4, US5)
- Each task includes: Goal, Components, Dependencies, Effort (S/M/L), Acceptance, Tests

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and baseline developer workflow

- [X] T001 Create backend and frontend project skeleton in backend/src/Program.cs [Goal: establish runnable webapp split | Components: backend, frontend | Depends: none | Effort: S | Acceptance: both apps start locally | Tests: smoke run commands pass]
- [X] T002 [P] Create frontend app shell and routing scaffold in frontend/src/app/router.tsx [Goal: baseline navigation | Components: React app shell | Depends: T001 | Effort: S | Acceptance: app routes render | Tests: frontend smoke test]
- [X] T003 [P] Configure Tailwind and shadcn baseline in frontend/src/app/styles.css [Goal: design system readiness | Components: Tailwind, shadcn | Depends: T002 | Effort: S | Acceptance: shadcn component renders with expected styling | Tests: visual smoke]
- [X] T004 [P] Configure CI quality pipeline in .github/workflows/ci.yml [Goal: automated checks per push | Components: unit, integration, contract jobs | Depends: T001 | Effort: M | Acceptance: CI runs lint and tests | Tests: CI dry run]
- [X] T005 Configure environment template in .env.example [Goal: consistent local setup | Components: DB, JWT, app vars | Depends: T001 | Effort: S | Acceptance: quickstart env vars covered | Tests: startup with env template]

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core capabilities required before user stories

**CRITICAL**: User-story implementation starts only after this phase

- [X] T006 Implement PostgreSQL DbContext and migration baseline in backend/src/persistence/QuitlyDbContext.cs [Goal: persistence foundation | Components: EF Core 10, Npgsql | Depends: T001, T005 | Effort: M | Acceptance: initial migration applies successfully | Tests: migration integration test]
- [X] T007 [P] Implement authentication endpoints in backend/src/api/AuthEndpoints.cs [Goal: register/login support | Components: JWT, password hashing | Depends: T001, T005 | Effort: M | Acceptance: register/login returns valid tokens | Tests: auth integration tests]
- [X] T008 [P] Implement auth middleware and ownership policy in backend/src/infrastructure/security/OwnershipPolicy.cs [Goal: resource isolation | Components: authz middleware | Depends: T007 | Effort: M | Acceptance: cross-user access denied | Tests: authorization integration tests]
- [X] T009 Create core domain entities for MVP in backend/src/domain/entities/User.cs [Goal: shared model baseline | Components: User, Habit, CheckIn, Trigger, Streak, Reminder, WeeklyInsight | Depends: T006 | Effort: M | Acceptance: entities map to DB schema | Tests: persistence mapping tests]
- [X] T010 [P] Implement API error envelope and validation pipeline in backend/src/infrastructure/http/ErrorHandlingMiddleware.cs [Goal: predictable API behavior | Components: validation + exception mapping | Depends: T001 | Effort: S | Acceptance: consistent 4xx/5xx payloads | Tests: API error contract tests]
- [X] T011 [P] Add API client, interceptors, and token refresh handling in frontend/src/services/httpClient.ts [Goal: reliable frontend-backend communication | Components: Axios, auth interceptor | Depends: T007 | Effort: M | Acceptance: authenticated requests refresh token seamlessly | Tests: frontend integration tests]
- [X] T012 Implement privacy-safe KPI aggregation job in backend/src/application/metrics/KpiAggregationJob.cs [Goal: KPI without analytics events | Components: operational-table aggregation | Depends: T009 | Effort: M | Acceptance: KPI snapshots generated from product data only | Tests: aggregation unit/integration tests]
- [X] T039 [P] Implement guard to disable analytics consent UI/API in MVP in frontend/src/features/privacy/ConsentGate.tsx [Goal: explicit FR-018b implementation | Components: UI/API guard flags | Depends: T011 | Effort: S | Acceptance: no consent dialog or consent endpoint exposed in MVP mode | Tests: UI/API negative smoke test]
- [X] T040 [P] Add negative tests for absent analytics consent surfaces in frontend/tests/integration/privacy-consent.absence.spec.ts [Goal: explicit FR-018b verification | Components: integration + contract-negative checks | Depends: T039 | Effort: S | Acceptance: tests prove consent UI/API are absent in MVP | Tests: integration/contract negative tests]

**Checkpoint**: Foundation complete; user story phases can proceed

---

## Phase 3: User Story 1 - Onboarding and Habit Goal (Priority: P1) MVP

**Goal**: New users can define one active habit goal and start quickly

**Independent Test**: New user completes onboarding and sees active goal in dashboard

### Tests for US1

- [X] T013 [P] [US1] Add contract tests for habit endpoints in backend/tests/contract/HabitEndpoints.contract.test.cs [Goal: lock API behavior | Components: GET/PUT /habit | Depends: T007, T009 | Effort: S | Acceptance: schema and status codes match contract | Tests: contract tests fail-before-pass]
- [X] T014 [P] [US1] Add onboarding E2E flow test in frontend/tests/e2e/onboarding.spec.ts [Goal: verify full user journey | Components: onboarding UI + API | Depends: T011 | Effort: M | Acceptance: end-to-end onboarding passes | Tests: Playwright]

### Implementation for US1

- [X] T015 [US1] Implement habit application service in backend/src/application/habits/HabitService.cs [Goal: enforce single active habit rule | Components: service + repository | Depends: T009 | Effort: M | Acceptance: replacing active habit deactivates previous one | Tests: service unit tests]
- [X] T016 [US1] Implement habit endpoints in backend/src/api/HabitEndpoints.cs [Goal: expose onboarding goal APIs | Components: GET/PUT /habit | Depends: T015, T010 | Effort: S | Acceptance: authenticated users can upsert/read active habit | Tests: endpoint integration tests]
- [X] T017 [US1] Implement onboarding page and form in frontend/src/features/onboarding/OnboardingPage.tsx [Goal: clear goal setup UX | Components: form, validation, copy | Depends: T011 | Effort: M | Acceptance: valid goal saved and confirmation shown | Tests: RTL component tests]
- [X] T018 [US1] Wire onboarding query/mutation layer in frontend/src/features/onboarding/onboarding.api.ts [Goal: stable data sync | Components: TanStack Query + Axios | Depends: T016, T017 | Effort: S | Acceptance: optimistic updates and error states handled | Tests: frontend integration tests]

**Checkpoint**: US1 independently functional and testable

---

## Phase 4: User Story 2 - Daily Check-In and Strict Streak (Priority: P1)

**Goal**: Users can submit daily check-ins and see strict streak updates

**Independent Test**: User records check-ins over 3 days and streak updates per rules

### Tests for US2

- [X] T019 [P] [US2] Add contract tests for check-in and streak endpoints in backend/tests/contract/CheckInStreak.contract.test.cs [Goal: lock key API contracts | Components: /check-ins, /streak | Depends: T009, T010 | Effort: M | Acceptance: contract tests pass against OpenAPI | Tests: contract tests]
- [X] T020 [P] [US2] Add streak rule unit tests in backend/tests/unit/StreakCalculatorTests.cs [Goal: deterministic streak logic | Components: abstinent/non-abstinent/missing day cases | Depends: T009 | Effort: S | Acceptance: all edge cases covered | Tests: xUnit unit tests]

### Implementation for US2

- [X] T021 [US2] Implement check-in service with correction semantics in backend/src/application/checkins/CheckInService.cs [Goal: single effective check-in per day | Components: upsert + correction logic | Depends: T009 | Effort: M | Acceptance: corrections overwrite prior same-day status | Tests: service unit tests]
- [X] T022 [US2] Implement streak calculator and materialization in backend/src/application/streaks/StreakService.cs [Goal: strict streak policy | Components: reset/interrupt rules | Depends: T021, T020 | Effort: M | Acceptance: non-abstinent day resets to 0 | Tests: unit + integration]
- [X] T023 [US2] Implement check-in and streak endpoints in backend/src/api/CheckInEndpoints.cs [Goal: expose daily loop API | Components: POST/GET check-ins, GET streak | Depends: T021, T022 | Effort: M | Acceptance: endpoints match contract and auth rules | Tests: endpoint integration tests]
- [X] T024 [US2] Implement check-in UI flow in frontend/src/features/checkin/CheckInPage.tsx [Goal: quick daily reflection | Components: status, mood, triggers, note | Depends: T011, T023 | Effort: M | Acceptance: submit and correction UX works | Tests: RTL + integration]
- [X] T025 [US2] Implement streak card UI in frontend/src/features/streak/StreakCard.tsx [Goal: visible progress state | Components: current streak + last status hints | Depends: T023 | Effort: S | Acceptance: streak reflects backend updates | Tests: component tests]

**Checkpoint**: US2 independently functional and testable

---

## Phase 5: User Story 3 - Minimal Relapse Recovery (Priority: P1)

**Goal**: Users can mark relapse and complete one non-shaming recovery step

**Independent Test**: User marks relapse and completes a 24h recovery step in one flow

### Tests for US3

- [X] T041 [P] [US3] Add contract tests for relapse and recovery endpoints in backend/tests/contract/RelapseRecovery.contract.test.cs [Goal: lock recovery contracts | Components: /relapse, /recovery-steps | Depends: T009, T010 | Effort: S | Acceptance: schema/status/security checks pass | Tests: contract tests]
- [X] T042 [P] [US3] Add integration test for relapse->recovery completion in backend/tests/integration/RelapseRecoveryFlowTests.cs [Goal: prove continuity behavior | Components: relapse mark + next-step completion | Depends: T009 | Effort: M | Acceptance: completion within 24h tracked correctly | Tests: integration tests]

### Implementation for US3

- [X] T043 [US3] Implement relapse and recovery services in backend/src/application/recovery/RecoveryService.cs [Goal: minimal non-shaming recovery logic | Components: relapse mark, next-step, completion | Depends: T021, T009 | Effort: M | Acceptance: users can create and complete recovery step | Tests: service unit tests]
- [X] T044 [US3] Implement relapse and recovery endpoints in backend/src/api/RecoveryEndpoints.cs [Goal: expose MVP recovery APIs | Components: POST /relapse, POST /recovery-steps | Depends: T043 | Effort: S | Acceptance: endpoints enforce auth/ownership and contract | Tests: endpoint integration tests]
- [X] T045 [US3] Implement recovery UI flow in frontend/src/features/recovery/RecoveryFlowPage.tsx [Goal: neutral, non-shaming UX | Components: relapse mark + one-step recovery | Depends: T044, T024 | Effort: M | Acceptance: user completes recovery flow from check-in context | Tests: frontend integration + E2E]

**Checkpoint**: US3 independently functional and testable

---

## Phase 6: User Story 5 - Passive Prompts and Weekly Insights (Priority: P2)

**Goal**: Users get passive in-app prompts and meaningful weekly insights

**Independent Test**: User sees passive prompt on missing check-in and weekly insight summary

### Tests for US5

- [X] T026 [P] [US5] Add contract tests for prompt and weekly insight endpoints in backend/tests/contract/PromptInsight.contract.test.cs [Goal: lock prompt/insight contracts | Components: /prompts/*, /insights/weekly | Depends: T009, T010 | Effort: S | Acceptance: contract tests pass | Tests: contract tests]
- [X] T027 [P] [US5] Add weekly insight aggregation tests in backend/tests/unit/WeeklyInsightAggregatorTests.cs [Goal: trend correctness with sparse data | Components: confidence and fallback summaries | Depends: T009 | Effort: M | Acceptance: low/medium/high confidence scenarios pass | Tests: unit tests]

### Implementation for US5

- [X] T028 [US5] Implement passive prompt service and preference handling in backend/src/application/prompts/PromptService.cs [Goal: respectful non-pushy prompts | Components: prompt eligibility + preferences | Depends: T009 | Effort: M | Acceptance: prompt shown only when check-in missing and enabled | Tests: service tests]
- [X] T029 [US5] Implement weekly insight aggregation service in backend/src/application/insights/WeeklyInsightService.cs [Goal: actionable weekly summary | Components: trends, top triggers, summary text | Depends: T009, T012, T043 | Effort: M | Acceptance: weekly insight persisted and retrievable | Tests: integration tests]
- [X] T030 [US5] Implement prompt and insight endpoints in backend/src/api/InsightPromptEndpoints.cs [Goal: expose US5 APIs | Components: GET today prompt, PUT prefs, GET weekly insight | Depends: T028, T029 | Effort: S | Acceptance: endpoints meet contract and auth checks | Tests: endpoint integration tests]
- [X] T031 [US5] Implement dashboard prompt and weekly insights UI in frontend/src/features/insights/WeeklyInsightsPage.tsx [Goal: low-cognitive-load insight UX | Components: prompt banner + summary cards | Depends: T030 | Effort: M | Acceptance: prompt + insight render with empty-data fallback | Tests: frontend integration tests]

**Checkpoint**: US5 independently functional and testable

---

## Phase 7: Post-MVP Backlog (Deferred Stories)

**Purpose**: Prepare extension tracks without activating in MVP

- [X] T033 [US4] Define intervention entity and migrations as disabled feature flag in backend/src/domain/entities/Intervention.cs [Goal: extension readiness | Components: Intervention model + migration | Depends: T009 | Effort: S | Acceptance: schema present but APIs disabled | Tests: migration tests]
- [X] T034 [US4] Add guarded route placeholders in frontend/src/features/postmvp/PostMvpPlaceholder.tsx [Goal: avoid accidental MVP scope drift | Components: disabled routes + messaging | Depends: T002 | Effort: S | Acceptance: routes hidden unless feature flag enabled | Tests: route guard tests]
- [X] T046 [US4] Add achievement scope decision record in specs/001-habit-reduction-mvp/research.md [Goal: explicit Post-MVP decision checkpoint | Owner: Product Lead | Decision deadline: end of Sprint 3 (pre-MVP-release) | Components: product decision note + acceptance criteria + trigger conditions | Depends: none | Effort: S | Acceptance: decision status, owner, deadline and trigger conditions documented | Tests: review checklist]

---

## Phase 8: Polish and Cross-Cutting Concerns

**Purpose**: Production readiness and ship confidence

- [X] T035 [P] Accessibility audit fixes in frontend/src/components/accessibility/ [Goal: WCAG 2.2 AA conformance | Components: keyboard nav, labels, contrast | Depends: T017, T024, T031 | Effort: M | Acceptance: automated and manual a11y checks pass | Tests: axe + manual checklist]
- [X] T036 [P] Performance budget enforcement in frontend/tests/integration/performance.spec.ts [Goal: meet P95 interaction/render targets | Components: perf smoke + thresholds | Depends: T024, T031 | Effort: M | Acceptance: budgets pass in CI | Tests: perf tests]
- [X] T037 Security hardening and rate limit policies in backend/src/infrastructure/security/RateLimitingExtensions.cs [Goal: reduce abuse and auth risk | Components: auth throttle, secure headers | Depends: T007, T008 | Effort: S | Acceptance: throttling and headers verified | Tests: security integration tests]
- [X] T038 Run full quickstart and release checklist in specs/001-habit-reduction-mvp/quickstart.md [Goal: ship-ready validation | Components: end-to-end verification + rollback notes | Depends: T035, T036, T037 | Effort: S | Acceptance: checklist completed with evidence links | Tests: full regression run]

---

## Dependencies and Execution Order

### Phase Dependencies

- Phase 1 -> Phase 2 -> (US1, US2, US3, US5 in priority order) -> Phase 8
- Phase 7 is intentionally deferred and does not block MVP shipping

### User Story Dependencies

- US1 (P1): starts immediately after Phase 2
- US2 (P1): depends on US1 API/auth baseline but can overlap once T016 is stable
- US3 (P1): depends on US2 check-in context and auth baseline
- US5 (P2): depends on US2 check-in data model and recovery data for continuity metrics
- US4: Post-MVP only, independent of MVP release

### Critical Path (MVP)

`T001 -> T006 -> T007 -> T009 -> T015 -> T016 -> T021 -> T022 -> T023 -> T024 -> T043 -> T044 -> T045 -> T029 -> T030 -> T031 -> T038`

---

## Parallel Opportunities

- Setup: `T002`, `T003`, `T004` after `T001`
- Foundation: `T007`, `T008`, `T010`, `T011` after baseline tasks
- US1 tests and implementation overlap: `T013`, `T014` parallel with `T015`
- US2 tests in parallel: `T019`, `T020`
- US3 tests in parallel: `T041`, `T042`
- US5 tests in parallel: `T026`, `T027`
- Polish parallel: `T035`, `T036`, `T037`

## Parallel Example: US2

```bash
Task: T019 Contract tests for check-in/streak endpoints
Task: T020 Streak rule unit tests
Task: T021 Check-in service implementation
```

## Sprint Proposal (3-4 Sprints, prioritized)

### Sprint 1 (Ship foundation + first usable slice)

- Scope: T001-T018
- Outcome: authenticated onboarding and active habit goal end-to-end
- Exit criteria: US1 independently demoable

### Sprint 2 (Core daily loop)

- Scope: T019-T025
- Outcome: check-in lifecycle and strict streak behavior live
- Exit criteria: US2 independently demoable

### Sprint 3 (Recovery + insight loop + release hardening)

- Scope: T041-T045, T026-T031, T035-T038
- Outcome: minimal recovery flow, passive prompts, weekly insights, quality gates, release readiness
- Exit criteria: MVP shippable with full evidence and relapse-path tests

### Sprint 4 (Optional / Post-MVP track)

- Scope: T033-T034, T046
- Outcome: extension scaffolding for interventions and achievement decision track
- Exit criteria: Post-MVP backlog technically seeded

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2
2. Deliver US1, then US2, then US3, then US5
3. Complete Phase 7 and ship

### Incremental Delivery

1. Sprint 1 release candidate: onboarding baseline
2. Sprint 2 release candidate: daily loop and streak reliability
3. Sprint 3 production candidate: insights + quality hardening

### Team Parallelization

- Developer A: backend services and endpoints
- Developer B: frontend feature flows
- Developer C: quality gates, CI, contract and integration tests
