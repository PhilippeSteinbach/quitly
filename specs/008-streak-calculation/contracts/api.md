# API Contracts: 008 — Streak- und Abstinenz-Berechnung

**Phase**: 1 | **Date**: 2026-05-30 | **Branch**: `008-streak-calculation`

All endpoints require JWT Bearer authentication unless noted. All timestamps in ISO 8601 UTC.
Error format: `{ "type": string, "title": string, "status": int, "detail": string }`.

---

## 1. GET /habits/{habitId}/streak

Returns the current live streak state for a habit. `currentStreakSeconds` is always server-verified (ground truth). **Manipulation detection is client-side**: after receiving this response, the client compares `serverUtcMs` against the expected time from its monotonic snapshot (`lastSnapshot.serverUtcMs + (performance.now() - lastSnapshot.performanceNowMs)`). A negative delta > 300 000 ms triggers the non-shaming correction toast. No `offlineDeltaMs` request parameter or `serverCorrectionFlag` response field is required.

### Response `200 OK`

```json
{
  "habitId": "uuid",
  "startedAt": "2026-01-15T09:30:00Z",
  "currentStreakSeconds": 1234567,
  "serverUtcMs": 1748000000000,
  "lastSyncAt": "2026-05-30T14:00:00Z",
  "mode": "quit"
}
```

| Field | Type | Notes |
|---|---|---|
| `currentStreakSeconds` | long | Seconds since last relapse (or startedAt if no relapse). Recalculated on each call. |
| `serverUtcMs` | long | Server's current UTC time in ms since epoch. Used by frontend for monotonic store sync. |
| `mode` | string | `"quit"` or `"reduce"` |

### Errors
- `404` — habit not found or not owned by caller
- `403` — habit belongs to another user

---

## 2. GET /habits/{habitId}/calendar/{year}/{month}

Returns day-level abstinence status for a given calendar month.

### Path params
- `year`: 4-digit integer (e.g. 2026)
- `month`: 1–12

### Response `200 OK`

```json
{
  "habitId": "uuid",
  "year": 2026,
  "month": 5,
  "userTimezone": "Europe/Berlin",
  "days": [
    { "date": "2026-05-01", "status": "abstinent", "relapses": [] },
    { "date": "2026-05-10", "status": "relapse",   "relapses": [
        { "occurredAt": "2026-05-10T18:45:00Z", "contextNote": "Stress bei der Arbeit" }
      ]
    },
    { "date": "2026-05-31", "status": "neutral",   "relapses": [] }
  ]
}
```

| Field | Notes |
|---|---|
| `status` | `"abstinent"` \| `"relapse"` \| `"paused"` \| `"neutral"` |
| `relapses` | Non-empty only for relapse days. All relapses of that day, chronological. `contextNote` is decrypted server-side before sending (sent over TLS). |
| `userTimezone` | IANA timezone string from user profile; used by client for rendering. |

Days before `startedAt` and future days have `status: "neutral"`.

### Errors
- `400` — invalid year/month
- `404` — habit not found

---

## 3. GET /habits/{habitId}/stats/{year}/{month}

Returns monthly abstinence statistics.

### Response `200 OK`

```json
{
  "habitId": "uuid",
  "year": 2026,
  "month": 5,
  "abstinentDays": 23,
  "relevantDays": 25,
  "relapseCount": 2,
  "isCurrentMonth": true
}
```

`relevantDays` = days from max(startedAt date, month start) to min(today, month end).  
`isCurrentMonth` = true if the requested month is the current calendar month.

---

## 4. GET /habits/{habitId}/stats/{year}

Returns yearly statistics — one MonthStats per calendar month.

### Response `200 OK`

```json
{
  "habitId": "uuid",
  "year": 2026,
  "totalAbstinentDays": 142,
  "totalRelevantDays": 150,
  "months": [
    { "month": 1, "abstinentDays": 0,  "relevantDays": 0,  "relapseCount": 0 },
    { "month": 5, "abstinentDays": 23, "relevantDays": 25, "relapseCount": 2 }
  ]
}
```

Months before habit start have `abstinentDays: 0, relevantDays: 0`.

---

## 5. POST /habits/{habitId}/relapses

Record a new relapse. Replaces the previous ad-hoc pattern of storing relapses via CheckIn.

### Request body

```json
{
  "occurredAt": "2026-05-30T18:45:00Z",
  "contextNote": "Stress bei der Arbeit"
}
```

| Field | Required | Notes |
|---|---|---|
| `occurredAt` | yes | Must not be in the future; must not be before habit `startedAt`. |
| `contextNote` | no | Max 500 chars. Encrypted at rest server-side. |

### Response `201 Created`

```json
{
  "id": "uuid",
  "occurredAt": "2026-05-30T18:45:00Z",
  "previousStreakSeconds": 1234567
}
```

### Errors
- `400` — `occurredAt` in future, before startedAt, or contextNote > 500 chars
- `404` — habit not found

---

## 6. Server Time endpoint (public)

### GET /time

Returns current server UTC time. Used for manipulation detection after offline period.

### Response `200 OK`

```json
{ "utcMs": 1748000000000 }
```

No authentication required. Rate-limited to 60 req/min per IP.

---

## Frontend TypeScript DTOs

```typescript
// src/services/streak.api.ts

export interface StreakDto {
  habitId: string;
  startedAt: string;           // ISO 8601 UTC
  currentStreakSeconds: number;
  serverUtcMs: number;
  lastSyncAt: string;
  mode: 'quit' | 'reduce';
}

export interface CalendarDayDto {
  date: string;                // YYYY-MM-DD
  status: 'abstinent' | 'relapse' | 'paused' | 'neutral';
  relapses: RelapseDetailDto[];
}

export interface RelapseDetailDto {
  occurredAt: string;          // ISO 8601 UTC
  contextNote?: string;
}

export interface CalendarMonthDto {
  habitId: string;
  year: number;
  month: number;
  userTimezone: string;
  days: CalendarDayDto[];
}

export interface MonthStatsDto {
  habitId: string;
  year: number;
  month: number;
  abstinentDays: number;
  relevantDays: number;
  relapseCount: number;
  isCurrentMonth: boolean;
}

export interface YearStatsDto {
  habitId: string;
  year: number;
  totalAbstinentDays: number;
  totalRelevantDays: number;
  months: Array<{ month: number; abstinentDays: number; relevantDays: number; relapseCount: number }>;
}
```
