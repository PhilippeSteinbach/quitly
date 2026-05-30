import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi, beforeEach, afterEach } from "vitest";
import { StreakCard } from "@/features/streak/StreakCard";

// Mock monotonic store so tests don't depend on Date.now()/performance.now()
vi.mock("@/lib/streak-calc/monotonic-store", () => ({
  getNowUtcMs: () => 1_000_000_000, // fixed epoch
  saveSnapshot: vi.fn(),
  loadSnapshot: vi.fn(() => null),
}));

const BASE_SERVER_UTC_MS = 1_000_000_000;
const BASE_STARTED_AT_MS = BASE_SERVER_UTC_MS - 86400 * 1000 * 3; // 3 days ago

describe("StreakCard", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("renders the day count from currentStreakSeconds", () => {
    render(
      <StreakCard
        currentStreakSeconds={86400 * 4} // 4 days
        serverUtcMs={BASE_SERVER_UTC_MS}
        startedAtMs={BASE_STARTED_AT_MS}
      />
    );

    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText(/tage?/i)).toBeInTheDocument();
  });

  it("renders hours:minutes:seconds format", () => {
    render(
      <StreakCard
        currentStreakSeconds={3661} // 1h 1m 1s
        serverUtcMs={BASE_SERVER_UTC_MS}
        startedAtMs={BASE_STARTED_AT_MS}
      />
    );

    // Should contain time formatted as HH:MM:SS
    expect(screen.getByText("01:01:01")).toBeInTheDocument();
  });

  it("renders singular 'Tag' for exactly 1 day", () => {
    render(
      <StreakCard
        currentStreakSeconds={86400}
        serverUtcMs={BASE_SERVER_UTC_MS}
        startedAtMs={BASE_STARTED_AT_MS}
      />
    );

    expect(screen.getByText("Tag")).toBeInTheDocument();
  });

  it("has aria-live region for screen readers", () => {
    render(
      <StreakCard
        currentStreakSeconds={0}
        serverUtcMs={BASE_SERVER_UTC_MS}
        startedAtMs={BASE_STARTED_AT_MS}
      />
    );

    expect(screen.getByRole("region", { name: /streak/i })).toBeInTheDocument();
  });
});

