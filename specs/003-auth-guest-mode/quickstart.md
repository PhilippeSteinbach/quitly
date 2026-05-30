# Quickstart: Auth & Guest Mode (Feature 003)

**Feature Branch**: `003-auth-guest-mode`  
**Built on top of**: `002-login-dashboard` (MVP core journey)

---

## Prerequisites

| Tool | Min Version | Check |
|------|-------------|-------|
| Node.js | 22 LTS | `node --version` |
| npm | 10 | `npm --version` |
| .NET SDK | 10 | `dotnet --version` |
| PostgreSQL | 17 | `psql --version` (or Docker) |
| Docker (optional) | 25 | `docker --version` |

---

## 1. Clone and Switch Branch

```bash
git checkout 003-auth-guest-mode
```

---

## 2. Backend Setup

### 2a. Configure environment

```bash
cd backend
cp appsettings.Development.json.example appsettings.Development.json
# Edit appsettings.Development.json — set ConnectionStrings.Default and Jwt.* values
```

Minimum required settings:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=quitly;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "<32-char-min-random-string>",
    "Issuer": "quitly-api",
    "Audience": "quitly-app",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30
  }
}
```

### 2b. Apply migrations (includes RefreshToken table)

```bash
cd backend/src
dotnet ef database update
```

### 2c. Run the API

```bash
dotnet run --project backend/src/Quitly.Api.csproj
# API listens on http://localhost:5200
```

---

## 3. Frontend Setup

### 3a. Install dependencies

```bash
cd frontend
npm install
# This pulls primereact, primeicons, and all existing deps
```

### 3b. Configure environment

```bash
cp .env.example .env.local
# VITE_API_BASE_URL=http://localhost:5200/api/v1  (default; only change if API port differs)
```

### 3c. Run the dev server

```bash
npm run dev
# Vite dev server on http://localhost:5173
```

---

## 4. Exploring the Feature

### Guest Mode Flow

1. Open `http://localhost:5173` → **Welcome page**
2. Click **"Continue as Guest"**
3. Complete onboarding (habit is stored in localStorage under `quitly.guest.habit`)
4. Do a daily check-in → stored under `quitly.guest.checkins`
5. Inspect the streak card → computed from localStorage, no API calls
6. Open DevTools → Application → Local Storage to inspect raw data

### Auth Flow

1. Click **"Create account"** on Welcome page
2. Fill email + password → redirected to onboarding on success
3. All subsequent API calls carry `Authorization: Bearer <accessToken>`
4. Logout → localStorage cleared, back to Welcome page

### Token Refresh (automated)

The Axios interceptor in `httpClient.ts` silently retries once using the refresh token on
any 401 response. To test manually:

```js
// In DevTools console while authenticated:
localStorage.setItem('quitly.access-token', 'expired-token');
// Trigger any API call → interceptor will refresh and retry automatically
```

---

## 5. Running Tests

### Frontend unit tests

```bash
cd frontend
npm test               # Vitest in watch mode
npm run test -- --run  # Single pass (CI)
```

**Critical guest-mode tests** (must exist and pass before implementation):
- `src/features/guest/guestStorage.spec.ts` – storage read/write/upsert invariants
- `src/features/guest/guestStreak.spec.ts` – streak calculation (TDD, property-based)

### Backend unit tests

```bash
cd backend
dotnet test tests/unit
```

**Critical token tests**:
- `TokenServiceTests.cs` – access token generation, refresh rotation, revocation

### Contract tests

```bash
cd backend
dotnet test tests/contract
```

**Critical contract tests**:
- `AuthEndpoints.contract.test.cs` – /auth/refresh, /auth/me, DELETE /auth/session response shapes

---

## 6. Key Source Locations

```
frontend/src/
├── features/auth/
│   ├── WelcomePage.tsx          # Entry: Login | Register | Continue as Guest
│   ├── LoginPage.tsx            # PrimeReact Card + InputText + Password + Button
│   ├── RegisterPage.tsx         # PrimeReact Card + InputText + Password + Button
│   ├── AuthContext.tsx          # React context + provider (mode, user)
│   ├── useAuth.ts               # Hook consuming AuthContext
│   ├── auth.api.ts              # TanStack Query mutations: login, register, refresh, logout
│   ├── GuestModeBanner.tsx      # PrimeReact Tag showing "Guest mode" in navbar
│   ├── RequireSession.tsx       # Route guard: allows guest + authenticated
│   ├── RequireAuth.tsx          # Route guard: authenticated only
│   └── index.ts                 # Barrel export of public auth API
├── features/guest/
│   ├── guestStorage.ts          # localStorage read/write for GuestHabit, GuestCheckIn
│   ├── guestStreak.ts           # Pure streak computation from GuestCheckIn[]
│   ├── guestExport.ts           # buildGuestExport + downloadGuestExport (JSON backup)
│   ├── useGuestHabit.ts         # TanStack Query wrappers for guest habit
│   ├── useGuestCheckIns.ts      # TanStack Query wrappers for guest check-ins
│   └── useGuestStreak.ts        # Derived streak query from localStorage
└── services/
    └── httpClient.ts            # (modified) silent refresh interceptor added

backend/src/
├── Api/
│   └── AuthEndpoints.cs         # (extended) /refresh, /me, /session DELETE
├── Domain/Entities/
│   └── RefreshToken.cs          # New entity
└── Persistence/
    └── Migrations/              # New migration: AddRefreshTokens
```

---

## 7. Design Decisions Quick Reference

| Topic | Choice | See |
|-------|--------|-----|
| UI library | PrimeReact 10.9.x, lara-light-cyan theme | research.md R-01 |
| Guest storage | localStorage (`quitly.guest.*`) | research.md R-02 |
| Auth state | React Context + localStorage | research.md R-03 |
| Token refresh | Silent Axios interceptor + `/auth/refresh` | research.md R-04 |
| Guest migration | JSON export on registration | research.md R-05 |
| Route protection | `RequireAnySession` / `RequireAuth` | research.md R-06 |
