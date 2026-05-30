/**
 * Guest mode localStorage CRUD utilities.
 * All keys are namespaced under `quitly.guest.*`.
 * No network calls are made from this module.
 */

const KEYS = {
  habit: "quitly.guest.habit",
  checkins: "quitly.guest.checkins",
  noticeDismissed: "quitly.guest.notice-dismissed"
} as const;

// ─── Types ───────────────────────────────────────────────────────────────────

export type GuestHabit = {
  title: string;
  category: string;
  timezone: string;
  createdAt: string; // ISO UTC
};

export type GuestCheckIn = {
  date: string; // YYYY-MM-DD in the user's timezone
  status: "abstinent" | "non_abstinent" | "unsure";
  mood: "very_low" | "low" | "neutral" | "good" | "very_good";
  triggers: string[];
  note: string;
};

// ─── Habit ───────────────────────────────────────────────────────────────────

export function getGuestHabit(): GuestHabit | null {
  try {
    const raw = localStorage.getItem(KEYS.habit);
    return raw ? (JSON.parse(raw) as GuestHabit) : null;
  } catch {
    return null;
  }
}

export function saveGuestHabit(habit: GuestHabit): void {
  try {
    localStorage.setItem(KEYS.habit, JSON.stringify(habit));
  } catch {
    // storage quota exceeded — silently ignore
  }
}

// ─── Check-ins ───────────────────────────────────────────────────────────────

export function getGuestCheckIns(): GuestCheckIn[] {
  try {
    const raw = localStorage.getItem(KEYS.checkins);
    return raw ? (JSON.parse(raw) as GuestCheckIn[]) : [];
  } catch {
    return [];
  }
}

/**
 * Upsert: one entry per date (same date = replace, not append).
 */
export function upsertGuestCheckIn(checkIn: GuestCheckIn): void {
  try {
    const existing = getGuestCheckIns();
    const index = existing.findIndex((c) => c.date === checkIn.date);
    if (index >= 0) {
      existing[index] = checkIn;
    } else {
      existing.push(checkIn);
    }
    localStorage.setItem(KEYS.checkins, JSON.stringify(existing));
  } catch {
    // storage quota exceeded — silently ignore
  }
}

// ─── Clear all guest data ─────────────────────────────────────────────────────

export function clearGuestData(): void {
  try {
    localStorage.removeItem(KEYS.habit);
    localStorage.removeItem(KEYS.checkins);
    localStorage.removeItem(KEYS.noticeDismissed);
  } catch {
    // ignore
  }
}

// ─── Notice dismissed ────────────────────────────────────────────────────────

export function isNoticeDismissed(): boolean {
  try {
    return localStorage.getItem(KEYS.noticeDismissed) === "true";
  } catch {
    return false;
  }
}

export function dismissNotice(): void {
  try {
    localStorage.setItem(KEYS.noticeDismissed, "true");
  } catch {
    // ignore
  }
}
