# Feature Specification: Auth & Guest Mode

**Feature Branch**: `003-auth-guest-mode`

**Created**: 2026-05-30

**Status**: Approved

**Input**: "Add auth (login/register) and a guest mode to Quitly. Guest mode must allow full offline habit tracking without an account. Authenticated users get JWT session with silent token refresh and logout. All new UI pages use PrimeReact components exclusively. Use TanStack Query and Axios for API communication."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Welcome & Auth Entry Point (Priority: P1)

As a new or returning user I want to land on a Welcome page that clearly offers me three
paths — Log in, Create account, or Continue as guest — so I can start using the app in
seconds without being forced to register first.

**Why this priority**: The Welcome page is the first screen every user sees. Without it,
no other user story in this feature can be reached. It directly implements constitution
Principle I (registration MUST remain optional).

**Independent Test**: A user who opens the app cold (no tokens, no guest key) sees the
Welcome page with three action buttons. Clicking "Continue as guest" transitions to guest
mode and lands on the Onboarding page. Clicking "Log in" shows the Login page. Clicking
"Create account" shows the Register page. All three paths are reachable and navigable
without any backend being available.

**Acceptance Scenarios**:

1. **Given** a user with no stored tokens and no guest key, **When** they open the app,
   **Then** they are redirected to `/welcome` and see Login, Register, and "Continue as
   Guest" options rendered with PrimeReact `Card` and `Button` components.
2. **Given** the Welcome page, **When** the user clicks "Continue as Guest", **Then**
   `quitly.auth-mode` is set to `'guest'` in localStorage and the user is navigated to
   `/onboarding` without any API call.
3. **Given** the Welcome page, **When** the user clicks "Log in" and submits valid
   credentials, **Then** a JWT access token and refresh token are stored and the user is
   navigated to `/onboarding`.
4. **Given** the Welcome page, **When** the user submits the Login form with incorrect
   credentials, **Then** a PrimeReact `Message` (inline) and/or `Toast` displays the
   error without exposing whether the email or password was wrong.
5. **Given** the Welcome page, **When** the user completes the Register form, **Then**
   a new account is created, tokens are stored, and the user lands on `/onboarding`.
6. **Given** a user already holding a valid access token in localStorage, **When** they
   navigate to `/welcome`, **Then** they are redirected away to `/onboarding` (no
   double-login).

---

### User Story 2 - Guest Mode Offline Tracking (Priority: P1)

As a guest user I want to set a habit goal, do daily check-ins, and see my streak — all
stored locally on my device — so I can get value from Quitly immediately without creating
an account.

**Why this priority**: Constitution Principle I mandates guest mode as fully functional for
all local-only features. It is the core trust promise: "registration MUST remain optional".
Without this story, the feature is not constitution-compliant.

**Independent Test**: With no backend running and `quitly.auth-mode = 'guest'`, a user can
complete onboarding (habit stored to `quitly.guest.habit`), submit three daily check-ins
(stored to `quitly.guest.checkins`), and see a correct streak count — all without any
network request.

**Acceptance Scenarios**:

1. **Given** guest mode is active, **When** the user completes onboarding, **Then** the
   habit is saved to `quitly.guest.habit` in localStorage and no API call is made.
2. **Given** a guest habit exists, **When** the user submits a daily check-in, **Then**
   the check-in is saved to `quitly.guest.checkins` with upsert semantics (one entry per
   calendar day), and the streak card updates immediately.
3. **Given** three consecutive abstinent check-ins, **When** the user views the streak
   card, **Then** `currentDays` is 3, computed by `guestStreak.ts`, with no API call.
4. **Given** a relapse check-in on day 2 of 3, **When** the streak is computed, **Then**
   `currentDays` resets to 0 per the clarified streak rule (any relapse = reset).
5. **Given** a guest user who had an abstinent check-in 2 days ago but no check-in
   yesterday, **When** the streak is computed today, **Then** `currentDays` is 0 (the
   missed day breaks the streak — same rule as the server-side `StreakService`).
6. **Given** guest mode, **When** the user navigates to a core tracking route
   (`/check-in`, `/recovery`, `/insights`), **Then** they are not redirected to login
   (RequireAnySession guard passes for guest mode).
7. **Given** guest mode, **When** the user navigates to an account-only route, **Then**
   they are redirected to `/welcome` (RequireAuth guard).

---

### User Story 3 - Authenticated Session: Refresh & Logout (Priority: P2)

As a registered user I want my session to stay alive silently across browser refreshes and
short inactivity periods, and I want a clear Logout action that cleanly ends my session on
both client and server.

**Why this priority**: Without silent refresh, authenticated users are forcibly logged out
every 15 minutes, making the app unusable. Logout is a security and data-control necessity
(constitution Principle I: permanent deletion and data control reachable in ≤ 3 taps).

**Independent Test**: After login, invalidate the access token in localStorage. The next
API call (e.g., fetching the habit) should silently refresh the token via
`POST /auth/refresh` and complete successfully — without the user seeing a login screen.
Logout from the navbar clears both tokens and revokes the refresh token server-side.

**Acceptance Scenarios**:

1. **Given** an authenticated user whose access token has just expired, **When** any API
   call is made, **Then** the Axios interceptor calls `POST /auth/refresh`, stores the
   new tokens, and retries the original request — all transparently.
2. **Given** the interceptor attempts a refresh and the refresh token is also expired or
   revoked, **When** the refresh call returns 401, **Then** all tokens are cleared and
   the user is redirected to `/welcome` with a PrimeReact `Toast` message.
3. **Given** an authenticated user, **When** they click Logout in the navbar, **Then**
   `DELETE /auth/session` is called with the current refresh token, both tokens are
   cleared from localStorage, the user is redirected to `/welcome`, and re-entering the
   app shows the Welcome page (not the old session).
4. **Given** an authenticated user reloads the page, **When** a valid access token is in
   localStorage, **Then** `GET /auth/me` is called to rehydrate `AuthContext` with the
   user's identity — no re-login required.
5. **Given** the "Guest mode" `Tag` is visible in the navbar for guest users, **When**
   the user upgrades to a registered account, **Then** the `Tag` disappears and the
   navbar reflects the authenticated state.

---

### User Story 4 - Guest-to-Registered Migration Prompt (Priority: P3)

As a guest user who decides to create an account, I want to be offered a JSON export of my
local data before the guest keys are cleared, so I never lose tracking data I've already
entered.

**Why this priority**: Constitution Principle I requires every user to be able to export
their full tracking data. This is especially critical at the guest→registered transition
when localStorage is about to be cleared.

**Independent Test**: A guest user with a saved habit and two check-ins clicks "Create
account". Before the registration form submits, a banner offers "Download your local data".
After successful registration, `quitly.guest.*` keys are cleared; the downloaded JSON
contains the habit and check-ins.

**Acceptance Scenarios**:

1. **Given** a guest user with local data, **When** they navigate to the Register page,
   **Then** a PrimeReact `Message` banner informs them that local data remains on-device
   and offers a "Download JSON backup" button.
2. **Given** the guest data export is triggered, **When** the user clicks "Download JSON
   backup", **Then** a JSON file matching the `GuestExport` schema is downloaded with
   `exportedAt`, `habit`, and `checkIns` fields.
3. **Given** a guest user with NO local data (no habit saved), **When** they open the
   Register page, **Then** no export banner is shown.
4. **Given** successful registration by a former guest, **When** the JWT tokens are stored,
   **Then** all `quitly.guest.*` localStorage keys are removed and `quitly.auth-mode` is
   set to `'authenticated'`.

---

### Edge Cases

- Guest user clears browser storage manually: app falls back to `unauthenticated` mode and
  shows the Welcome page on next visit (no crash).
- Guest user submits two check-ins on the same calendar day: second write upserts the
  first (uniqueness invariant on `(habitId, date)` enforced in `guestStorage.ts`).
- Authenticated user's refresh token is revoked server-side (e.g., password change on
  another device): interceptor detects 401 on `/auth/refresh`, clears client state,
  redirects to Welcome page with a friendly `Toast`.
- Register form submitted with an email already taken: backend returns `409 Conflict` with
  `{ error: "email_taken" }`; frontend displays it as a PrimeReact `Message` below the
  email field — no generic "Something went wrong" message.
- User navigates directly to `/check-in` without any session: `RequireAnySession` guard
  catches it and redirects to `/welcome`.
- PrimeReact `Password` field strength meter: disabled (`feedback={false}`) on Login,
  enabled on Register (visual guide, not a blocker — any password ≥ 8 chars is accepted).
