import { afterEach, describe, expect, it } from "vitest";
import {
  clearGuestData,
  dismissNotice,
  getGuestCheckIns,
  getGuestHabit,
  isNoticeDismissed,
  saveGuestHabit,
  upsertGuestCheckIn
} from "@/features/guest/guestStorage";
import type { GuestCheckIn, GuestHabit } from "@/features/guest/guestStorage";

const testHabit: GuestHabit = {
  title: "No cigarettes",
  category: "smoking",
  timezone: "UTC",
  createdAt: "2025-01-01T00:00:00.000Z"
};

const testCheckIn: GuestCheckIn = {
  date: "2025-01-10",
  status: "abstinent",
  mood: "neutral",
  triggers: [],
  note: ""
};

afterEach(() => {
  localStorage.clear();
});

describe("getGuestHabit", () => {
  it("returns null when nothing is stored", () => {
    expect(getGuestHabit()).toBeNull();
  });

  it("returns the saved habit", () => {
    saveGuestHabit(testHabit);
    expect(getGuestHabit()).toEqual(testHabit);
  });

  it("returns null after localStorage.clear() without throwing", () => {
    saveGuestHabit(testHabit);
    localStorage.clear();
    expect(getGuestHabit()).toBeNull();
  });
});

describe("getGuestCheckIns", () => {
  it("returns [] when nothing is stored", () => {
    expect(getGuestCheckIns()).toEqual([]);
  });

  it("returns [] after localStorage.clear() without throwing", () => {
    upsertGuestCheckIn(testCheckIn);
    localStorage.clear();
    expect(getGuestCheckIns()).toEqual([]);
  });
});

describe("upsertGuestCheckIn", () => {
  it("appends a new check-in", () => {
    upsertGuestCheckIn(testCheckIn);
    expect(getGuestCheckIns()).toHaveLength(1);
  });

  it("replaces existing entry for the same date (upsert invariant)", () => {
    upsertGuestCheckIn(testCheckIn);
    const updated: GuestCheckIn = { ...testCheckIn, status: "non_abstinent" };
    upsertGuestCheckIn(updated);
    const checkIns = getGuestCheckIns();
    expect(checkIns).toHaveLength(1);
    expect(checkIns[0].status).toBe("non_abstinent");
  });

  it("keeps multiple check-ins on different dates", () => {
    upsertGuestCheckIn(testCheckIn);
    upsertGuestCheckIn({ ...testCheckIn, date: "2025-01-11" });
    expect(getGuestCheckIns()).toHaveLength(2);
  });

  it("key isolation: habit key is unaffected by check-in upserts", () => {
    saveGuestHabit(testHabit);
    upsertGuestCheckIn(testCheckIn);
    expect(getGuestHabit()).toEqual(testHabit);
  });
});

describe("clearGuestData", () => {
  it("removes habit, checkins, and notice-dismissed keys", () => {
    saveGuestHabit(testHabit);
    upsertGuestCheckIn(testCheckIn);
    dismissNotice();
    clearGuestData();
    expect(getGuestHabit()).toBeNull();
    expect(getGuestCheckIns()).toEqual([]);
    expect(isNoticeDismissed()).toBe(false);
  });
});

describe("notice dismissed flag", () => {
  it("returns false when not dismissed", () => {
    expect(isNoticeDismissed()).toBe(false);
  });

  it("returns true after dismiss", () => {
    dismissNotice();
    expect(isNoticeDismissed()).toBe(true);
  });
});
