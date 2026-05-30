import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { YearHeatmap } from "@/features/streak/YearHeatmap";
import type { MonthStatsDto } from "@/services/streak.api";

function makeMonth(month: number, abstinentDays: number, relevantDays: number): MonthStatsDto {
  return { year: 2026, month, abstinentDays, relevantDays, relapseCount: 0, isCurrentMonth: false };
}

const ALL_MONTHS: MonthStatsDto[] = Array.from({ length: 12 }, (_, i) =>
  makeMonth(i + 1, 25, 30)
);

describe("YearHeatmap", () => {
  it("renders 12 tiles", () => {
    render(
      <YearHeatmap year={2026} months={ALL_MONTHS} startedOnDate="2026-01-01" />
    );
    const tiles = screen.getAllByRole("img");
    expect(tiles).toHaveLength(12);
  });

  it("neutral tiles before startedOnDate have 'Neutral' label", () => {
    // Habit started in June 2026 → Jan–May should be neutral
    render(
      <YearHeatmap year={2026} months={ALL_MONTHS} startedOnDate="2026-06-01" />
    );
    const janTile = screen.getByLabelText("Jan: Neutral");
    expect(janTile).toBeInTheDocument();

    const mayTile = screen.getByLabelText("Mai: Neutral");
    expect(mayTile).toBeInTheDocument();
  });

  it("non-neutral tiles after startedOnDate show percentage", () => {
    render(
      <YearHeatmap year={2026} months={ALL_MONTHS} startedOnDate="2026-01-01" />
    );
    // 25/30 ≈ 83 %
    expect(screen.getAllByLabelText(/83 % abstinent/)).not.toHaveLength(0);
  });

  it("tiles with 0 relevantDays render as neutral", () => {
    const months = [...ALL_MONTHS];
    months[0] = makeMonth(1, 0, 0);
    render(
      <YearHeatmap year={2026} months={months} startedOnDate="2026-01-01" />
    );
    expect(screen.getByLabelText("Jan: Neutral")).toBeInTheDocument();
  });

  it("100 % rate tile shows 100 %", () => {
    const months = ALL_MONTHS.map((m, i) =>
      makeMonth(i + 1, i === 0 ? 31 : 25, i === 0 ? 31 : 30)
    );
    render(
      <YearHeatmap year={2026} months={months} startedOnDate="2026-01-01" />
    );
    expect(screen.getByLabelText(/Jan.*100 % abstinent/)).toBeInTheDocument();
  });
});
