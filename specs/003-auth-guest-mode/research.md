# Research: Auth & Guest Mode (Feature 003)

**Feature Branch**: `003-auth-guest-mode`  
**Date**: 2026-05-30  
**Phase**: 0 – Unknowns resolved before design

---

## R-01: PrimeReact Integration with Vite + React + TailwindCSS

**Decision**: Add PrimeReact 10.9.x in Styled mode (lara-light-cyan theme) alongside the
existing Tailwind CSS setup. The new auth/guest-mode pages use PrimeReact exclusively; the
already-implemented pages from feature 001 keep their Tailwind styling unchanged.

**Rationale**: User directive is "almost never vanilla HTML/CSS/JS; always PrimeReact
components and styling system". PrimeReact 10.9.8 supports React 19 and Vite without extra
config. Styled mode provides accessible, keyboard-navigable components with minimal
boilerplate. lara-light-cyan fits a wellness/recovery app tone (calm, supportive palette).
Coexistence with Tailwind is supported: import PrimeReact theme CSS before Tailwind base to
prevent specificity conflicts.

**Alternatives considered**:
- Continue ShadCN/Tailwind for auth pages (rejected: violates user directive)
- PrimeReact unstyled + Tailwind pass-through (rejected: high setup cost, contradicts the
  "rely on PrimeReact styling" directive)
- PrimeFlex as layout utility (considered: being sunset; Tailwind or PrimeReact's flex
  utilities used instead)

**Packages required**:
```
primereact@^10.9.8
primeicons@^7.0.0
```

**Setup in main.tsx**:
```tsx
import { PrimeReactProvider } from 'primereact/api';
import 'primereact/resources/themes/lara-light-cyan/theme.css';
import 'primeicons/primeicons.css';
```

**Key components for this feature**:
- `Card` – wraps auth forms
- `InputText` + `FloatLabel` – email input
- `Password` – masked password with toggleMask
- `Button` – submit, ghost actions
- `Message` / `Toast` – error and success feedback
- `Divider` – visual separator between login/register/guest options
- `ProgressSpinner` – loading indicator
- `Panel` / `Toolbar` – page layout in auth shell
- `ConfirmDialog` – "Clear guest data?" confirmation (constitution Principle II)
- `Tag` – "Guest mode" status indicator in navbar

---

## R-02: Guest Mode Architecture — Client-Side Only

**Decision**: Guest mode is 100% client-side. Guest data lives in localStorage. No backend
API calls are made for guest users. The backend is not changed to support anonymous users.

**Rationale**: Constitution Principle I mandates that guest mode is "fully functional for all
local-only features (tracking, counters, achievements, motivation text)". Achieving this via
backend anonymous sessions would introduce account-less data ownership problems and a privacy
surface. localStorage is the simplest correct model for offline-first local tracking.

**Alternatives considered**:
- Anonymous backend sessions (rejected: privacy risk, ownership ambiguity, server-side
  complexity)
- IndexedDB for guest storage (rejected: overkill for MVP data volumes; localStorage is
  sufficient for one habit + daily check-ins for 365 days)
- sessionStorage (rejected: data lost on tab close, which would break streak continuity)

**localStorage key schema** (see data-model.md for schemas):
```
quitly.auth-mode          → 'guest' | 'authenticated'
quitly.guest.habit        → JSON: GuestHabit | null
quitly.guest.checkins     → JSON: GuestCheckIn[]
quitly.access-token       → string (JWT, already implemented)
quitly.refresh-token      → string (already implemented)
```

**Security note**: localStorage is accessible to any script on the same origin; acceptable for
this non-sensitive data. No PII (email/password) is stored in guest localStorage keys.

---

## R-03: Auth State Management

**Decision**: `AuthContext` (React context + hook) holds auth state in memory, hydrated from
localStorage on mount. No global state library (Zustand, Redux) introduced.

**Rationale**: Auth state is read broadly across the app but mutates infrequently (login,
logout, guest start). A React context is the idiomatic, dependency-free solution. The context
is the single source of truth; localStorage is the persistence backing.

**AuthState shape**:
```ts
type AuthMode = 'unauthenticated' | 'guest' | 'authenticated';

type AuthState = {
  mode: AuthMode;
  user: { id: string; email: string } | null;
};
```

Access token is NOT stored in AuthState; it is read from tokenStorage (httpClient.ts) on each
request. This avoids stale-token bugs from context re-renders.

**Alternatives considered**:
- Zustand (rejected: adds dependency; context sufficient for this use case)
- Cookie-based sessions (rejected: CORS complexity with separate .NET API origin)
- JWT in-memory only with no localStorage (rejected: page refresh loses auth, creating a
  broken UX; refresh-token flow mitigates the security downside of localStorage storage)

---

## R-04: Token Refresh — New Backend Endpoint Required

**Decision**: Add `POST /api/v1/auth/refresh` to the backend. The httpClient interceptor
will attempt a silent refresh before redirecting to login on 401.

**Rationale**: The existing httpClient.ts 401 interceptor clears tokens and does not retry.
For production-ready auth, a silent refresh cycle (access token short-lived, refresh token
longer-lived) is required. Without it, users are logged out every 15–60 minutes.

**New backend endpoints** (see contracts/auth-api.yaml for full spec):
- `POST /api/v1/auth/refresh` – accepts `{ refreshToken }`, returns new `{ accessToken, refreshToken }`
- `GET /api/v1/auth/me` – returns `{ id, email, timezone, locale }` for the authenticated user
- `DELETE /api/v1/auth/session` – revokes refresh token (logout)

**Alternatives considered**:
- Very long-lived access tokens (rejected: violates OWASP session management best practices)
- No refresh — force re-login (rejected: poor UX, especially on mobile sessions)

---

## R-05: Guest-to-Registered Migration

**Decision**: When a guest starts registration, offer a JSON export of their guest data.
Automatic server-side import is deferred to Post-MVP.

**Rationale**: Constitution Principle I ("every user must be able to export their full
tracking data in a machine-readable format"). A guest completing registration should not
silently lose their local data. A JSON export satisfies data ownership requirements. Full
cloud-import of guest history (with conflict resolution) is complex and Post-MVP.

**Migration flow**:
1. Guest taps "Create account"
2. App shows banner: "Your local data will remain on this device. Download a copy?"
3. User can download JSON (habit + check-ins)
4. Registration proceeds; guest keys cleared on successful registration

**Alternatives considered**:
- Automatic server import (deferred: conflict resolution rules needed per Principle III)
- No migration path (rejected: violates Principle I data ownership)
- Merge guest data silently into the new account (rejected: may conflict with server state
  if user previously had an account)

---

## R-06: Route Guard Strategy

**Decision**: Two guard levels — `RequireAuth` (redirects to `/welcome` if not authenticated
OR guest) for post-MVP account features, and `RequireAnySession` (redirects to `/welcome` if
`mode === 'unauthenticated'`) for core tracking features that work in guest mode.

**Rationale**: Constitution Principle I: core tracking (viewing counter, starting track,
reporting relapse, viewing achievements, editing motivation) MUST work offline and in guest
mode. Only cloud-sync and leaderboard features require a registered account.

**MVP guard matrix**:

| Route | RequireAnySession | RequireAuth |
|-------|:-----------------:|:-----------:|
| `/welcome` | – | – |
| `/onboarding` | ✅ | – |
| `/check-in` | ✅ | – |
| `/recovery` | ✅ | – |
| `/insights` | ✅ | – |
| `/settings` | ✅ | – |
| `/account` | – | ✅ |

**Alternatives considered**:
- No route guards (rejected: unauthenticated user hits API, gets 401, bad UX)
- Full RequireAuth on all routes (rejected: violates constitution Principle I)
