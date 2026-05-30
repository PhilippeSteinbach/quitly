/**
 * T018 – TDD gate: these tests must FAIL before guestStreak.ts is implemented.
 * Run: npx vitest run src/features/guest/guestStreak.spec.ts
 */
import { describe, expect, it } from "vitest";
import { computeGuestStreak } from "@/features/guest/guestStreak";
import type { GuestCheckIn } from "@/features/guest/guestStorage";

// Helper to build a check-in on a given YYYY-MM-DD date
function ci(date: string, status: GuestCheckIn["status"] = "abstinent"): GuestCheckIn {
  return { date, status, mood: "neutral", triggers: [], note: "" };
}

describe("computeGuestStreak", () => {
  it("returns 0 streak for empty check-ins", () => {
    const result = computeGuestStreak([], "2025-01-10", "UTC");
    expect(result.currentDays).toBe(0);
    expect(result.longestDays).toBe(0);
  });

  it("counts consecutive abstinent days ending today", () => {
    const checkIns = [ci("2025-01-08"), ci("2025-01-09"), ci("2025-01-10")];
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(3);
  });

  it("resets current streak to 0 after a non-abstinent day", () => {
    const checkIns = [ci("2025-01-08"), ci("2025-01-09", "non_abstinent"), ci("2025-01-10")];
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(1);
  });

  it("resets current streak to 0 after an unsure day", () => {
    const checkIns = [ci("2025-01-08"), ci("2025-01-09", "unsure"), ci("2025-01-10")];
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(1);
  });

  it("missing-day gap rule: no check-in yesterday resets streak to 0", () => {
    // US2 AC5 — gap between last check-in and today means streak is 0
    const checkIns = [ci("2025-01-08")]; // 2 days ago, no yesterday
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(0);
  });

  it("missing-day gap rule: check-in today but not yesterday still counts as 1", () => {
    const checkIns = [ci("2025-01-08"), ci("2025-01-10")]; // gap on Jan 9
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(1);
  });

  it("tracks longest streak across all history", () => {
    const checkIns = [
      ci("2025-01-01"),
      ci("2025-01-02"),
      ci("2025-01-03"),
      ci("2025-01-04", "non_abstinent"),
      ci("2025-01-05"),
      ci("2025-01-06")
    ];
    const result = computeGuestStreak(checkIns, "2025-01-06", "UTC");
    expect(result.longestDays).toBe(3);
    expect(result.currentDays).toBe(2);
  });

  it("handles DST boundary — clocks move forward (March, US timezones)", () => {
    // America/New_York spring forward Mar 10, 2024 — 3 consecutive days should still count as 3
    const checkIns = [ci("2024-03-09"), ci("2024-03-10"), ci("2024-03-11")];
    const result = computeGuestStreak(checkIns, "2024-03-11", "America/New_York");
    expect(result.currentDays).toBe(3);
  });

  it("handles leap year Feb 28 → Mar 1 boundary", () => {
    const checkIns = [ci("2024-02-28"), ci("2024-02-29"), ci("2024-03-01")];
    const result = computeGuestStreak(checkIns, "2024-03-01", "UTC");
    expect(result.currentDays).toBe(3);
  });

  it("returns longestDays = currentDays when there is only one unbroken run", () => {
    const checkIns = [ci("2025-01-09"), ci("2025-01-10")];
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.longestDays).toBe(2);
  });

  it("ignores check-ins after 'today'", () => {
    const checkIns = [ci("2025-01-09"), ci("2025-01-10"), ci("2025-01-11")];
    const result = computeGuestStreak(checkIns, "2025-01-10", "UTC");
    expect(result.currentDays).toBe(2);
  });
});
