# Tasks: Auth & Guest Mode

**Input**: Design documents from `/specs/003-auth-guest-mode/`

**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/auth-api.yaml ✅ | quickstart.md ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no unresolved dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Exact file paths are listed in every task

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add PrimeReact to the existing Vite + React workspace and create the folder
scaffolding used across all user stories.

- [X] T001 Install `primereact@^10.9.8` and `primeicons@^7.0.0` in `frontend/package.json`
- [X] T002 [P] Import PrimeReact theme and primeicons in `frontend/src/app/styles.css` (before Tailwind base) — `lara-light-cyan`
- [X] T003 [P] Wrap `<RouterProvider>` with `<PrimeReactProvider>` in `frontend/src/main.tsx`
- [X] T004 [P] Create folder `frontend/src/features/auth/` with empty `.gitkeep`
- [X] T005 [P] Create folder `frontend/src/features/guest/` with empty `.gitkeep`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Auth context, guest storage primitives, route guards, and backend token
infrastructure. Every user story depends on these.

⚠️ **CRITICAL**: No user story work can begin until Phase 2 is complete.

### Backend: RefreshToken entity & endpoints

- [X] T006 Create `backend/src/Domain/Entities/RefreshToken.cs` with fields: `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `CreatedAt`, `ReplacedById`
- [X] T007 Add `DbSet<RefreshToken> RefreshTokens` to `backend/src/Persistence/QuitlyDbContext.cs` and configure cascade-delete from `User`
- [X] T008 Add EF Core migration `AddRefreshTokens` — run `dotnet ef migrations add AddRefreshTokens` in `backend/src/`
- [X] T009 Extend `backend/src/Infrastructure/Security/TokenService.cs`: add `IssueRefreshToken()`, `RotateRefreshToken()`, `RevokeRefreshToken()` — raw token returned once, SHA-256 hash stored only
- [X] T010 Register route method groups for the three new auth endpoints in `backend/src/Api/AuthEndpoints.cs` — map `POST /auth/refresh`, `GET /auth/me`, `DELETE /auth/session` to stub handlers returning `501 Not Implemented`; wires routing/DI only; actual implementations delivered in T038–T040
- [X] T011 [P] Add rate-limit policy for `/auth/refresh` (10 req/min per IP) in `backend/src/Program.cs`

### Frontend: Auth context & HTTP client

- [X] T012 Create `frontend/src/features/auth/AuthContext.tsx` — `AuthMode` type (`'unauthenticated' | 'guest' | 'authenticated'`), `AuthState`, `AuthContext`, `AuthProvider` component; persists mode to `quitly.auth-mode` in localStorage
- [X] T013 Create `frontend/src/features/auth/useAuth.ts` — hook consuming `AuthContext`; throws if used outside `AuthProvider`
- [X] T014 Extend `frontend/src/services/httpClient.ts`: add silent-refresh Axios interceptor — on 401, call `POST /auth/refresh` once and retry the original request; on a second 401 (refresh also failed), clear tokens via `tokenStorage.clear()` and emit a custom DOM event `auth:session-expired`; `AuthContext` listens for this event and calls React Router's `navigate('/welcome')`; never use `window.location.href` directly (avoids React state desync)
- [X] T015 [P] Create `frontend/src/features/auth/auth.api.ts` — TanStack Query mutations: `useLoginMutation`, `useRegisterMutation`, `useRefreshMutation`, `useLogoutMutation`; `useCurrentUserQuery` (wraps `GET /auth/me`)

### Frontend: Guest storage primitives

- [X] T016 Create `frontend/src/features/guest/guestStorage.ts` — typed CRUD over `quitly.guest.habit` and `quitly.guest.checkins`; exports `getGuestHabit`, `saveGuestHabit`, `getGuestCheckIns`, `upsertGuestCheckIn`, `clearGuestData`; enforces one-entry-per-date upsert invariant
- [X] T017 Create `frontend/src/features/guest/guestStreak.ts` — pure function `computeGuestStreak(checkIns, today, timezone): GuestStreak`; no side effects; uses same rules as server `StreakService`

### ⚠️ TDD Gate (Constitution Principle V)

- [X] T018 Write **failing** unit tests for `guestStreak.ts` in `frontend/src/features/guest/guestStreak.spec.ts` — property-based tests covering: consecutive abstinent days, relapse resets to 0, missing-day gap (today vs yesterday), DST boundary, leap-year Feb 28/Mar 1, timezone change mid-streak — **tests must fail before T017 is implemented**
- [X] T019 [P] Write unit tests for `guestStorage.ts` in `frontend/src/features/guest/guestStorage.spec.ts` — upsert invariant (same date = replace), key isolation, null-safe getters; also cover: simulate `localStorage.clear()` between reads — verify `getGuestHabit()` returns `null` and `getGuestCheckIns()` returns `[]` without throwing

### Frontend: Route guards

- [X] T020 Create `frontend/src/features/auth/RequireSession.tsx` — redirects to `/welcome` when `mode === 'unauthenticated'`; passes guest and authenticated users through
- [X] T021 [P] Create `frontend/src/features/auth/RequireAuth.tsx` — redirects to `/welcome` when `mode !== 'authenticated'`
- [X] T022 Update `frontend/src/app/router.tsx` — add `/welcome`, `/login`, `/register` routes (public); wrap `/onboarding`, `/check-in`, `/recovery`, `/insights` with `<RequireSession>`; wrap `/account` with `<RequireAuth>`; set `/welcome` as default redirect from `/` when unauthenticated (do NOT add `/settings` — deferred to the settings feature)
- [X] T022b [P] Mount `/account` route in `frontend/src/app/router.tsx` pointing to the existing `PostMvpPlaceholder` component (`feature="Account"`) so `RequireAuth` has a valid destination and the router does not throw a match error

**Checkpoint**: Foundation complete — all user stories can now proceed in parallel.

---

## Phase 3: User Story 1 — Welcome & Auth Entry Point (Priority: P1) 🎯 MVP

**Goal**: A Welcome page with Login, Register, and "Continue as Guest" paths. All rendered
with PrimeReact components. No access to core routes without a session.

**Independent Test**: Open the app cold (no tokens, no guest key) → Welcome page appears.
"Continue as Guest" sets `quitly.auth-mode='guest'` and navigates to `/onboarding`. "Log
in" with valid credentials stores tokens and navigates to `/onboarding`. "Create account"
with a new email stores tokens and navigates to `/onboarding`. Invalid credentials show a
PrimeReact `Message` without revealing which field was wrong.

### Implementation — User Story 1

- [X] T023 [US1] Create `frontend/src/features/auth/WelcomePage.tsx` — PrimeReact `Card` layout with `Button` components for "Log in", "Create account", "Continue as Guest"; "Continue as Guest" calls `AuthContext.setMode('guest')` and navigates to `/onboarding`
- [X] T024 [P] [US1] Create `frontend/src/features/auth/LoginPage.tsx` — PrimeReact `Card`, `FloatLabel` + `InputText` for email, `Password` (`feedback={false}`, `toggleMask`), `Button` submit; calls `useLoginMutation`; on success stores tokens via `tokenStorage` and navigates to `/onboarding`; on error shows PrimeReact `Message`
- [X] T025 [P] [US1] Create `frontend/src/features/auth/RegisterPage.tsx` — PrimeReact `Card`, `FloatLabel` + `InputText` for email, `Password` (with strength feedback, `toggleMask`), `Button` submit; calls `useRegisterMutation`; on success stores tokens and navigates to `/onboarding`; on `email_taken` error shows field-level PrimeReact `Message`
- [X] T026 [US1] Add `<Toast>` ref to `frontend/src/app/App.tsx` and pass via context or prop for global async feedback (login error, session expired)
- [X] T027 [US1] Add `GuestModeBanner` — `frontend/src/features/auth/GuestModeBanner.tsx` — PrimeReact `Tag` (severity `warning`, label "Guest mode") shown in the App navbar when `mode === 'guest'`; update `frontend/src/app/App.tsx` to render it
- [X] T028 [P] [US1] Add inverse redirect guard to the `/welcome` route element in `frontend/src/app/router.tsx`: if `AuthContext.mode === 'authenticated'`, render `<Navigate to="/onboarding" replace />` — this is distinct from T022's `RequireSession` guards (which protect other routes from unauthenticated access); together they prevent a logged-in user from seeing the Welcome page

**Checkpoint**: US1 fully functional — user can reach any auth path from the Welcome page.

---

## Phase 4: User Story 2 — Guest Mode Offline Tracking (Priority: P1)

**Goal**: Guest users can complete onboarding, do daily check-ins, and view streak — all
stored in localStorage with zero network calls.

**Independent Test**: With the backend stopped and `quitly.auth-mode='guest'`, a user can
save a habit, submit three check-ins on three different dates, and see `currentDays: 3` in
the streak card. No network errors appear.

### Implementation — User Story 2

- [X] T029 [US2] Create `frontend/src/features/guest/useGuestHabit.ts` — TanStack Query `useQuery` + `useMutation` wrappers over `guestStorage.ts` that mirror the shape of `useActiveHabitQuery` / `useUpsertHabitMutation` from `onboarding.api.ts`
- [X] T030 [US2] Create `frontend/src/features/guest/useGuestCheckIns.ts` — TanStack Query wrappers over `guestStorage.ts` for check-in read and upsert; mirrors `checkin.api.ts` interface
- [X] T031 [US2] Create `frontend/src/features/guest/useGuestStreak.ts` — derives `GuestStreak` via `computeGuestStreak` from cached `quitly.guest.checkins`; returns same shape as server streak DTO
- [X] T032 [US2] Update `frontend/src/features/onboarding/OnboardingPage.tsx` — when `mode === 'guest'` use `useGuestHabit` hooks instead of `onboarding.api.ts` hooks; keep authenticated path unchanged
- [X] T033 [US2] Update `frontend/src/features/checkin/CheckInPage.tsx` — when `mode === 'guest'` use `useGuestCheckIns` hooks; when `mode === 'authenticated'` keep existing `checkin.api.ts` hooks
- [X] T034 [US2] Update `frontend/src/features/streak/StreakCard.tsx` — when `mode === 'guest'` use `useGuestStreak`; when authenticated use existing server streak query
- [X] T035 [P] [US2] Add passive "Your data is stored locally" notice (PrimeReact `Message`, severity `info`, closable) to `frontend/src/features/checkin/CheckInPage.tsx` — shown only in guest mode, first check-in only (dismissed state in `quitly.guest.notice-dismissed`)

**Checkpoint**: US2 fully functional — guest tracking works fully offline.

---

## Phase 5: User Story 3 — Authenticated Session: Refresh & Logout (Priority: P2)

**Goal**: Silent token refresh keeps authenticated sessions alive. Logout revokes the
server-side refresh token and clears client state.

**Independent Test**: After login, manually expire the access token in localStorage. Next
API call succeeds silently (interceptor refreshed it). Clicking Logout calls
`DELETE /auth/session`, clears localStorage, and renders the Welcome page.

### Backend tests (constitution Principle V — token logic)

- [X] T036 Write unit tests in `backend/tests/unit/TokenServiceTests.cs` — covers: token rotation (old token revoked, new issued), replay rejection (already-revoked token returns 401), expiry check, max-5-per-user cap (oldest revoked on overflow)
- [X] T037 [P] Write contract tests in `backend/tests/contract/AuthEndpoints.contract.test.cs` — validates `/auth/refresh`, `/auth/me`, `DELETE /auth/session` response shapes against `specs/003-auth-guest-mode/contracts/auth-api.yaml`

### Implementation — User Story 3

- [X] T038 [US3] Implement `POST /auth/refresh` handler in `backend/src/Api/AuthEndpoints.cs` — validates token hash against `RefreshToken` table, rejects revoked/expired, rotates (revoke old, issue new), enforces 5-token cap
- [X] T039 [P] [US3] Implement `GET /auth/me` handler in `backend/src/Api/AuthEndpoints.cs` — returns `UserProfile` for the authenticated user from the JWT claim
- [X] T040 [P] [US3] Implement `DELETE /auth/session` handler in `backend/src/Api/AuthEndpoints.cs` — revokes the provided refresh token; idempotent (200/204 if already revoked)
- [X] T041 [US3] Add `useCurrentUserQuery` call in `frontend/src/app/App.tsx` — on mount when `quitly.access-token` exists, call `GET /auth/me`, populate `AuthContext` with the returned user, and persist the `AuthUser` to `quitly.auth-user` in localStorage (enables synchronous navbar hydration on next reload before the `/auth/me` response arrives); on 401 clear tokens and set `mode='unauthenticated'` (depends on T026 — Toast must exist before session-expired Toast fires)
- [X] T042 [US3] Add Logout button to the App navbar in `frontend/src/app/App.tsx` — visible when `mode === 'authenticated'`; calls `useLogoutMutation` (which calls `DELETE /auth/session`), then clears `tokenStorage` and sets `mode='unauthenticated'` (depends on T041 — Logout uses the auth state populated by T041)

**Checkpoint**: US3 fully functional — sessions survive page refresh, logout is clean.

---

## Phase 6: User Story 4 — Guest-to-Registered Migration Prompt (Priority: P3)

**Goal**: Guest users with local data are offered a JSON export before registration clears
their localStorage.

**Independent Test**: As a guest with a saved habit, navigate to `/register`. A
PrimeReact `Message` banner appears with a "Download JSON backup" button. Clicking it
downloads a valid `GuestExport` JSON. After successful registration all `quitly.guest.*`
keys are cleared.

### Implementation — User Story 4

- [X] T043 [US4] Create `frontend/src/features/guest/guestExport.ts` — `buildGuestExport(): GuestExport` reads `quitly.guest.habit` + `quitly.guest.checkins`, returns typed `GuestExport` object; `downloadGuestExport()` triggers browser file download as `quitly-backup-<date>.json`
- [X] T043b [P] [US4] Write unit tests for `guestExport.ts` in `frontend/src/features/guest/guestExport.spec.ts` — cover: schema shape matches `GuestExport` type (all required fields present), null habit returns `{ habit: null, checkIns: [] }`, `formatVersion` is always `'1'`, `exportedAt` is a valid ISO UTC timestamp, filename contains today's date in YYYY-MM-DD format
- [X] T044 [US4] Update `frontend/src/features/auth/RegisterPage.tsx` — on mount check if `mode === 'guest'` AND `getGuestHabit() !== null`; if so render PrimeReact `Message` (severity `warn`) with a "Download JSON backup" `Button`; button calls `downloadGuestExport()`
- [X] T044b [US4] After successful registration and before calling `clearGuestData()`, show PrimeReact `ConfirmDialog` in `frontend/src/features/auth/RegisterPage.tsx`: message "Your local tracking data will be cleared from this device. This cannot be undone.", accept label "Continue", reject label "Cancel" — only proceed with T045's cleanup on user confirmation (constitution Principle II: destructive actions require explicit confirmation)
- [X] T045 [US4] After user confirms the dialog (T044b), call `clearGuestData()` from `guestStorage.ts` then set `mode='authenticated'` in `AuthContext` — skip this step if the user cancels the confirm dialog

**Checkpoint**: US4 fully functional — no guest data is silently lost during registration.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Accessibility, error hardening, and quickstart validation.

- [X] T046 [P] Add `axe-core` accessibility scan for `WelcomePage`, `LoginPage`, `RegisterPage` in the Vitest/Playwright CI step — verify WCAG 2.1 AA (touch targets ≥ 44×44, visible focus rings, all PrimeReact inputs have accessible labels)
- [X] T047 [P] Add E2E Playwright test `frontend/tests/e2e/auth.spec.ts` — covers: cold start → Welcome, guest flow (habit + check-in stored in localStorage with no network requests), login flow, logout flow (tokens cleared, server-side session revoked), and silent refresh (override access token to expired value via CDP/localStorage, trigger API call, verify request succeeds and new token stored without user seeing login screen)
- [X] T048 [P] Add E2E Playwright test `frontend/tests/e2e/guest-migration.spec.ts` — covers: guest with data → Register page shows export banner → download → registration clears guest keys
- [X] T049 Validate quickstart.md steps end-to-end: fresh clone → `npm install` → `dotnet run` → `npm run dev` → Welcome page loads; update `specs/003-auth-guest-mode/quickstart.md` if any step is wrong
- [X] T050 [P] Add `frontend/src/features/auth/` barrel export `index.ts` for public API: `AuthProvider`, `useAuth`, `RequireSession`, `RequireAuth`, `GuestModeBanner`

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup)
    └── Phase 2 (Foundational — blocks all stories)
            ├── Phase 3 (US1: Welcome & Auth Entry)     ← P1, start here
            ├── Phase 4 (US2: Guest Offline Tracking)   ← P1, can parallel with US1 after T016–T019
            ├── Phase 5 (US3: Refresh & Logout)         ← P2, start after US1 done
            └── Phase 6 (US4: Migration Prompt)         ← P3, start after US2 done
                    └── Phase 7 (Polish)
```

### Critical TDD gates

- **T018 must fail before T017** — `guestStreak.spec.ts` written before `guestStreak.ts`
- **T036 must fail before T038–T040** — `TokenServiceTests.cs` written before refresh/logout handlers

### Within-Phase Parallel Opportunities

| Phase | Parallel group |
|-------|----------------|
| Phase 2 | T006–T008 (backend entity) ‖ T012–T013 (auth context) ‖ T016 (guest storage) ‖ T018–T019 (TDD tests) ‖ T020–T021 (guards) |
| Phase 3 | T024 (LoginPage) ‖ T025 (RegisterPage) after T023 (WelcomePage) |
| Phase 4 | T029–T031 (guest hooks) can run in parallel then feed T032–T034 |
| Phase 5 | T039–T040 (backend) ‖ after T036–T037 tests pass |
| Phase 7 | T046, T047, T048, T050 all in parallel |

### Suggested MVP Scope

**MVP = Phase 1 + Phase 2 + Phase 3 + Phase 4**

This delivers the constitution-compliant guest mode (Principle I) plus a usable auth entry
point. US3 (token refresh) and US4 (migration) can ship as fast-follow.

---

## Parallel Execution Example: Phase 2 (Foundational)

```
Developer A                          Developer B                        Developer C
───────────────────────────          ───────────────────────────        ─────────────────────
T006 RefreshToken entity             T012 AuthContext.tsx               T016 guestStorage.ts
T007 DbContext update                T013 useAuth.ts                    T018 guestStreak.spec.ts (TDD)
T008 EF migration                    T014 httpClient.ts interceptor     T019 guestStorage.spec.ts
T009 TokenService extend             T015 auth.api.ts                   T017 guestStreak.ts
T010 AuthEndpoints extend            T020 RequireSession.tsx
T011 Rate limit                      T021 RequireAuth.tsx
                                     T022 router.tsx update
```
