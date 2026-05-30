# Data Model: Auth & Guest Mode (Feature 003)

**Feature Branch**: `003-auth-guest-mode`  
**Date**: 2026-05-30

---

## Overview

Feature 003 introduces two layers of data:

1. **Auth state** – which session mode the user is in and their identity
2. **Guest storage** – full local replica of the core tracking data for guest users

Guest storage mirrors the server-side entity shapes so that feature code (onboarding,
check-in, streak) can be written against a single, shape-consistent interface regardless of
auth mode.

---

## 1. Auth State (In-Memory + localStorage)

### Entity: `AuthState`

Held in React context (`AuthContext`). Persisted via `quitly.auth-mode` localStorage key.

```ts
type AuthMode = 'unauthenticated' | 'guest' | 'authenticated';

type AuthUser = {
  id: string;      // UUID (server-issued for authenticated users)
  email: string;
};

type AuthState = {
  mode: AuthMode;
  user: AuthUser | null;  // null in 'unauthenticated' and 'guest' modes
};
```

### localStorage keys

| Key | Type | Description |
|-----|------|-------------|
| `quitly.auth-mode` | `'guest' \| 'authenticated'` | Persisted mode. Absence = unauthenticated. |
| `quitly.access-token` | `string` | Short-lived JWT (already managed by `tokenStorage`) |
| `quitly.refresh-token` | `string` | Rotating refresh token (already managed by `tokenStorage`) |
| `quitly.auth-user` | `JSON: AuthUser` | Cached user identity for page reload without API round-trip |
| `quitly.guest.notice-dismissed` | `'true'` | Set when the "data stored locally" info notice in CheckInPage is dismissed; presence = dismissed |

### Transitions

```
unauthenticated ──── "Continue as Guest" ───→ guest
unauthenticated ──── Login / Register ───────→ authenticated
guest ───────────── "Create Account" ────────→ (export prompt) → authenticated
guest ───────────── "Sign In" ───────────────→ (export prompt) → authenticated
authenticated ───── Logout ──────────────────→ unauthenticated
authenticated ───── Token expired (no refresh)→ unauthenticated
```

---

## 2. Guest Storage (localStorage)

All guest keys are prefixed `quitly.guest.*`. They mirror the server entity shapes so
feature components work the same whether in guest or authenticated mode.

### Entity: `GuestHabit`

localStorage key: `quitly.guest.habit`

```ts
type GuestHabit = {
  id: string;           // UUID generated locally (crypto.randomUUID())
  category: HabitCategory;
  mode: HabitMode;      // 'quit' | 'reduce'
  title: string;        // max 120 chars
  startedOn: string;    // ISO date (YYYY-MM-DD) in user's local calendar
  motivationText: string | null;
  active: boolean;      // always true in MVP (one active habit)
  createdAt: string;    // ISO timestamp UTC
  updatedAt: string;    // ISO timestamp UTC
};
```

Mirrors server `HabitDto`. `startedOn` is a calendar date (not UTC instant) because
"day 1" is relative to the user's local midnight — consistent with the server-side
implementation.

### Entity: `GuestCheckIn`

localStorage key: `quitly.guest.checkins` (JSON array, newest-last)

```ts
type GuestCheckIn = {
  id: string;           // UUID generated locally
  habitId: string;      // foreign key to GuestHabit.id
  date: string;         // ISO date (YYYY-MM-DD) — one entry per calendar day
  status: 'abstinent' | 'relapse';
  mood: 1 | 2 | 3 | 4 | 5;  // MoodLevel
  triggers: string[];   // zero or more free-text triggers
  note: string | null;
  recordedAt: string;   // ISO timestamp UTC — when the check-in was saved
};
```

**Uniqueness invariant**: Only one `GuestCheckIn` per `(habitId, date)` tuple. Upsert
semantics on save (replace existing entry for the same date).

### Entity: `GuestStreak` (derived, never stored)

Computed in-memory from `GuestCheckIn[]` on every read. Stored streak values would drift
if the user doesn't check in for several days.

```ts
type GuestStreak = {
  currentDays: number;       // consecutive abstinent days ending today
  longestDays: number;       // all-time longest abstinent streak
  lastCheckinDate: string | null; // ISO date of the most recent check-in
  trend: 'stable' | 'improving' | 'declining';
};
```

**Streak calculation rules** (matching server-side `StreakService`):
- A "streak day" is a calendar day with `status === 'abstinent'`
- Streak resets to 0 on any day with `status === 'relapse'` (spec clarification from 001)
- A day with no check-in does NOT break the streak during the current calendar day, but
  does break it if the missing day was yesterday (constitution Principle III: accurate time)
- Time zone: computations use the IANA timezone from the user's `Intl.DateTimeFormat`

### Entity: `GuestExport`

Shape of the JSON file produced during guest-to-registered migration (see R-05).

```ts
type GuestExport = {
  exportedAt: string;    // ISO timestamp UTC
  formatVersion: '1';
  habit: GuestHabit | null;
  checkIns: GuestCheckIn[];
};
```

---

## 3. Backend Entities (new/modified)

### Modified: `User` entity

No new columns. The `User.PasswordHash` field already exists. Only a new refresh-token
store is needed.

### New: `RefreshToken` entity

Stored server-side to enable token revocation (logout) and rotation.

```
RefreshToken {
  Id           : Guid (PK)
  UserId       : Guid (FK → User.Id, cascade delete)
  TokenHash    : string (SHA-256 hash of the raw token)
  ExpiresAt    : DateTimeOffset
  RevokedAt    : DateTimeOffset? (null = still valid)
  CreatedAt    : DateTimeOffset
  ReplacedById : Guid? (FK → RefreshToken.Id, for rotation chain)
}
```

**Security note**: Raw refresh tokens are never stored. Only the SHA-256 hash is
persisted. The raw token is returned to the client once and never stored server-side.

---

## 4. Validation Rules

| Field | Rule |
|-------|------|
| `GuestHabit.title` | 2–120 characters |
| `GuestHabit.startedOn` | ISO date, not in the future |
| `GuestCheckIn.date` | ISO date, ≤ today (no future check-ins) |
| `GuestCheckIn.mood` | Integer 1–5 inclusive |
| `GuestCheckIn.triggers` | Max 10 items; each item max 80 chars |
| `GuestCheckIn.note` | Max 500 chars |
| `GuestHabit.motivationText` | Max 280 chars |
| `AuthUser.email` | Valid RFC 5322 email (trimmed, lowercased) |
| `RegisterRequest.password` | Min 8 chars (backend enforces; no additional complexity rules) |
| `RefreshToken.ExpiresAt` | 30 days from issuance |
| JWT access token TTL | 15 minutes |

---

## 5. Data Flow Diagram

```
                    ┌─────────────────────────────┐
                    │         WelcomePage          │
                    │  [Login] [Register] [Guest]  │
                    └──────────┬──────────┬────────┘
                               │          │
                    ┌──────────▼──┐  ┌────▼──────────────┐
                    │ Authenticated│  │   Guest Mode       │
                    │   Session    │  │  (localStorage)    │
                    └──────┬───────┘  └─────┬─────────────┘
                           │                │
              ┌────────────▼────────────────▼────────────┐
              │             AuthContext                   │
              │  mode: 'authenticated' | 'guest'          │
              └────────────────────┬──────────────────────┘
                                   │
              ┌────────────────────▼──────────────────────┐
              │          Feature API layer                  │
              │  auth.api.ts → HTTP (authenticated)         │
              │  guestStorage.ts → localStorage (guest)     │
              └───────────────────────────────────────────-┘
```
