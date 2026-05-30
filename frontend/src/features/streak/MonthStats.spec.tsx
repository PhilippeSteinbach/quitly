import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { MonthStats } from "@/features/streak/MonthStats";
import type { MonthStatsDto } from "@/services/streak.api";

const base: MonthStatsDto = {
  year: 2026,
  month: 5,
  abstinentDays: 23,
  relevantDays: 30,
  relapseCount: 7,
  isCurrentMonth: false,
};

describe("MonthStats", () => {
  it("renders 'X von Y Tagen abstinent'", () => {
    render(<MonthStats stats={base} />);
    expect(screen.getByText(/23/)).toBeInTheDocument();
    expect(screen.getByText(/von 30 Tagen abstinent/)).toBeInTheDocument();
  });

  it("shows relapse count", () => {
    render(<MonthStats stats={base} />);
    expect(screen.getByText(/7 Rückfälle/)).toBeInTheDocument();
  });

  it("shows 'Laufend' badge when isCurrentMonth", () => {
    render(<MonthStats stats={{ ...base, isCurrentMonth: true }} />);
    expect(screen.getByText("Laufend")).toBeInTheDocument();
  });

  it("hides 'Laufend' badge when not current month", () => {
    render(<MonthStats stats={base} />);
    expect(screen.queryByText("Laufend")).not.toBeInTheDocument();
  });

  it("uses today-capped denominator (relevantDays ≠ total days)", () => {
    // Habit started mid-month; denominator should be 16, not 31
    render(<MonthStats stats={{ ...base, relevantDays: 16, abstinentDays: 16, relapseCount: 0 }} />);
    expect(screen.getByText(/von 16 Tagen abstinent/)).toBeInTheDocument();
  });

  it("singular Rückfall for count = 1", () => {
    render(<MonthStats stats={{ ...base, relapseCount: 1 }} />);
    expect(screen.getByText(/1 Rückfall$/)).toBeInTheDocument();
  });
});
