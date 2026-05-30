# Implementation Plan: Auth & Guest Mode

**Branch**: `003-auth-guest-mode` | **Date**: 2026-05-30 | **Spec**: [specs/003-auth-guest-mode/spec.md](./spec.md)

**Input**: Feature context from constitution, branch `002-login-dashboard` codebase, and user directive (PrimeReact, TanStack Query, Axios, Vite).

## Summary

Feature 003 wires Quitly's authentication and guest mode into the frontend and extends the
backend with token refresh, user profile, and logout endpoints. New pages (Welcome, Login,
Register) are built exclusively with PrimeReact 10.9.x components. A guest mode lets users
track habits fully offline with localStorage — no account required. Authenticated users use
existing JWT-based auth with a new silent refresh cycle. Route guards enforce that core
tracking works in guest mode while account-only features (cloud sync, leaderboard) require
registration.

## Technical Context

**Language/Version**:
- Frontend: TypeScript 5.x, React 19.x, Vite 6.x
- Backend: C# 14 / .NET 10 (Minimal API)

**Primary Dependencies**:
- Frontend: PrimeReact 10.9.x (new), primeicons 7.x (new), TanStack Query 5.x, Axios 1.x,
  React Router DOM 7.x, Tailwind CSS 3.x (existing pages unchanged)
- Backend: ASP.NET Core Minimal API, EF Core 10, Npgsql, BCrypt/Argon2id

**Storage**:
- Authenticated users: PostgreSQL (existing) + new `RefreshToken` table
- Guest users: localStorage (`quitly.guest.*` key namespace)

**Testing**:
- Frontend: Vitest + React Testing Library (unit), Playwright (E2E critical paths)
- Backend: xUnit + FluentAssertions + Testcontainers
- Contract: OpenAPI schema validation (auth-api.yaml)

**Target Platform**:
- Responsive web (mobile-first), same as feature 001

**Project Type**:
- Web application (SPA frontend + .NET Minimal API backend)

**Performance Goals**:
- `POST /auth/login` and `POST /auth/refresh`: P95 ≤ 300 ms
- Guest mode check-in save (localStorage write): ≤ 16 ms (single frame)
- Welcome page first meaningful paint: ≤ 1.5 s (PrimeReact theme CSS lazy-loaded)

**Constraints**:
- Guest mode MUST work fully offline (constitution Principle IV)
- No PII stored in guest localStorage keys (email, password never in `quitly.guest.*`)
- WCAG 2.1 AA: all PrimeReact auth forms must pass automated accessibility checks
- No analytics events (constitution Principle I)
- Raw refresh tokens MUST NOT be stored server-side (hash only)

**Scale/Scope**:
- Same 5k–20k MAU assumption as feature 001
- Refresh token table: max 5 active tokens per user (oldest revoked on overflow)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Gate Assessment

| Principle | Status | Notes |
|-----------|--------|-------|
| I – Privacy & Data Ownership First | ✅ PASS | Guest mode is the central deliverable. No PII in guest keys. JSON export on registration satisfies data portability. |
| II – Empathy-Driven UX | ✅ PASS | Welcome page offers guest option prominently. No shame language. Guest data export dialog uses neutral framing ("Your local data"). Guest mode clear action uses PrimeReact `ConfirmDialog` (Principle II: destructive confirm). |
| III – Time-Accuracy | ⚠️ CONDITIONAL | Guest streak uses local calendar dates. UTC timestamps recorded at check-in. DST/timezone property-based tests required before implementation (see Principle V). |
| IV – Offline-First | ✅ PASS | Guest mode is inherently offline. Auth mode gracefully degrades: network failures produce `Toast` warning, never block writes. |
| V – Test-First for Streak/XP | ✅ PASS (gated) | `guestStreak.ts` must have failing tests written BEFORE implementation. Property-based tests required for DST transitions, leap years, timezone changes. |
| VI – Accessibility | ✅ PASS | PrimeReact 10.x components are ARIA-compliant. Float labels use proper `htmlFor`. Forms have explicit error `Message` components. WCAG 2.1 AA automated check in CI. |
| VII – Gamification Without Dark Patterns | N/A | No gamification in auth/guest mode. |

### Post-Phase 1 Re-Check

All gates pass post-design. The `RefreshToken` entity and guest storage schema introduce no
constitutional violations. The guest-to-registered migration flow (JSON export) satisfies
Principle I without adding complexity that would delay the feature.

## Project Structure

### Documentation (this feature)

```text
specs/003-auth-guest-mode/
├── plan.md              ← this file
├── research.md          ← Phase 0: decisions on PrimeReact, guest mode, token refresh
├── data-model.md        ← Phase 1: AuthState, GuestHabit, GuestCheckIn, RefreshToken
├── quickstart.md        ← Phase 1: dev setup and test guide
├── contracts/
│   └── auth-api.yaml    ← Phase 1: /auth/refresh, /auth/me, DELETE /auth/session
└── tasks.md             ← Phase 2 (created by /speckit.tasks — not part of this plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Api/
│   │   └── AuthEndpoints.cs          ← extended: /refresh, /me, DELETE /session
│   ├── Domain/Entities/
│   │   └── RefreshToken.cs           ← new entity
│   ├── Infrastructure/Security/
│   │   └── TokenService.cs           ← extended: refresh token issuance + rotation
│   └── Persistence/
│       ├── QuitlyDbContext.cs         ← new DbSet<RefreshToken>
│       └── Migrations/               ← new: AddRefreshTokens
└── tests/
    ├── unit/
    │   └── TokenServiceTests.cs      ← new: rotation, expiry, replay rejection
    └── contract/
        └── AuthEndpoints.contract.test.cs ← new: refresh, me, logout contracts

frontend/
├── src/
│   ├── app/
│   │   ├── App.tsx                   ← modified: PrimeReactProvider, auth-aware layout
│   │   ├── router.tsx                ← modified: /welcome route, route guards
│   │   └── styles.css                ← modified: PrimeReact theme CSS imported first
│   ├── features/
│   │   ├── auth/
│   │   │   ├── WelcomePage.tsx       ← new: Login | Register | Continue as Guest
│   │   │   ├── LoginPage.tsx         ← new: PrimeReact Card + InputText + Password
│   │   │   ├── RegisterPage.tsx      ← new: PrimeReact Card + InputText + Password
│   │   │   ├── AuthContext.tsx       ← new: React context provider (mode, user)
│   │   │   ├── useAuth.ts            ← new: hook consuming AuthContext
│   │   │   ├── auth.api.ts           ← new: login, register, refresh, logout mutations
│   │   │   ├── GuestModeBanner.tsx   ← new: PrimeReact Tag in navbar
│   │   │   ├── RequireSession.tsx    ← new: route guard (any session = guest or auth)
│   │   │   └── RequireAuth.tsx       ← new: route guard (authenticated only)
│   │   └── guest/
│   │       ├── guestStorage.ts       ← new: localStorage CRUD for habit + check-ins
│   │       └── guestStreak.ts        ← new: pure streak computation from check-ins
│   └── services/
│       └── httpClient.ts             ← modified: silent refresh interceptor
├── src/test/
│   └── features/
│       ├── auth/                     ← new: WelcomePage, auth flow unit tests
│       └── guest/                    ← new: guestStorage, guestStreak unit tests (TDD)
└── package.json                      ← modified: add primereact, primeicons
```

**Structure Decision**: Web application (frontend + backend), single repo. New files added to
the existing `frontend/src/features/` and `backend/src/` layout established in feature 001.
No new top-level directories.

## Auth & Security

- Authentication: email/password via existing `POST /auth/register` and `POST /auth/login`.
- Session model: 15-min JWT access token + 30-day rotating refresh token (new).
- Refresh tokens: SHA-256 hash stored server-side; raw token returned once to client.
- Replay protection: each refresh token is single-use; rotation immediately revokes old token.
- Max active tokens: 5 per user (oldest revoked on overflow to prevent token flooding).
- Rate limiting: `/auth/refresh` — 10 req/min per IP (existing ASP.NET rate limiter extended).
- Guest mode: no server calls; localStorage only; no PII in guest keys.
- Transport: TLS enforced, HSTS in production (inherited from feature 001).

## PrimeReact Integration Strategy

- Theme: `lara-light-cyan` — calm, wellness-appropriate palette.
- Import order in `main.tsx` / `styles.css`: PrimeReact theme → primeicons → Tailwind base.
  This prevents Tailwind's reset from stripping PrimeReact component styles.
- PrimeReact components used in this feature:
  - `Card` — form containers on Login, Register, Welcome pages
  - `InputText` + `FloatLabel` — email field
  - `Password` — password field with toggleMask; `feedback={false}` on Login
  - `Button` — primary and outlined/text variants for actions
  - `Message` — inline field error display
  - `Toast` — async operation feedback (login error, logout confirmation)
  - `Divider` — separator between "or continue as guest"
  - `ConfirmDialog` — "Clear guest data? This cannot be undone."
  - `Tag` — "Guest mode" indicator in the App navbar
  - `ProgressSpinner` — loading state on form submit
- Existing pages (OnboardingPage, CheckInPage, etc.) retain their Tailwind styling.
  No migration of existing components is in scope for this feature.

## Test Strategy

- **TDD gate** (Principle V): `guestStreak.ts` tests written before implementation.
  Property-based tests cover DST, leap years, timezone change mid-streak, offline-to-online.
- **Unit**: `guestStorage.spec.ts` (upsert invariant, key isolation), `guestStreak.spec.ts`
  (streak calculation), `AuthContext.spec.tsx` (mode transitions), `auth.api.spec.ts` (mutations).
- **Integration (backend)**: `TokenServiceTests.cs` (rotation, expiry, replay rejection),
  `AuthEndpoints.contract.test.cs` validates against `auth-api.yaml` schema.
- **E2E (Playwright)**: `auth.spec.ts` — register flow, login flow, guest flow,
  guest-to-registered migration prompt. Run in `frontend/tests/e2e/`.
- **Accessibility**: `axe` scan on `WelcomePage`, `LoginPage`, `RegisterPage` in CI.

## Risks and Mitigations

- **Risk**: PrimeReact + Tailwind CSS conflicts (specificity collisions on shared elements).
  - Mitigation: import PrimeReact theme CSS before Tailwind; scope PrimeReact pages to
    feature routes; add Playwright visual regression screenshot for Welcome page.
- **Risk**: Guest localStorage data lost on browser clear (user panic).
  - Mitigation: show passive "Your data is stored locally" notice on first guest check-in;
    surface JSON export option prominently in settings.
- **Risk**: Silent refresh interceptor causes infinite retry loop on hard auth failures.
  - Mitigation: interceptor retries exactly once; on second 401, clears tokens and redirects.
- **Risk**: Streak computation divergence between guest (local) and server (post-migration).
  - Mitigation: share streak calculation rules as a pure TypeScript function used by both
    `guestStreak.ts` and `StreakCard.tsx`; server remains authoritative post-sync.

## Complexity Tracking

> No unjustified constitutional violations identified.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| Two UI libraries (Tailwind + PrimeReact) | User directive requires PrimeReact; existing pages use Tailwind | Migrating existing components out of scope; coexistence is clean with correct import order |
