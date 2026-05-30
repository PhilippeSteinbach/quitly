import { render, screen, fireEvent } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { CalendarView } from "@/features/streak/CalendarView";
import type { CalendarDayDto } from "@/services/streak.api";

function makeDays(count: number, status: CalendarDayDto["status"] = "abstinent"): CalendarDayDto[] {
  return Array.from({ length: count }, (_, i) => ({
    date: `2026-05-${String(i + 1).padStart(2, "0")}`,
    status,
  }));
}

describe("CalendarView", () => {
  it("renders the correct number of day cells", () => {
    const days = makeDays(31);
    render(<CalendarView year={2026} month={5} days={days} />);
    const buttons = screen.getAllByRole("button");
    expect(buttons).toHaveLength(31);
  });

  it("renders correct aria-label for each day", () => {
    const days: CalendarDayDto[] = [
      { date: "2026-05-01", status: "abstinent" },
      { date: "2026-05-02", status: "relapse" },
    ];
    render(<CalendarView year={2026} month={5} days={days} />);
    expect(screen.getByLabelText("2026-05-01: Abstinent")).toBeInTheDocument();
    expect(screen.getByLabelText("2026-05-02: Rückfall")).toBeInTheDocument();
  });

  it("shows relapse detail when relapse day is clicked", () => {
    const days: CalendarDayDto[] = [
      { date: "2026-05-10", status: "relapse", notes: ["Stress bei der Arbeit"] },
    ];
    render(<CalendarView year={2026} month={5} days={days} />);
    fireEvent.click(screen.getByLabelText("2026-05-10: Rückfall"));
    expect(screen.getByText("Stress bei der Arbeit")).toBeInTheDocument();
  });

  it("shows 'Keine Notiz' for relapse without notes", () => {
    const days: CalendarDayDto[] = [{ date: "2026-05-10", status: "relapse" }];
    render(<CalendarView year={2026} month={5} days={days} />);
    fireEvent.click(screen.getByLabelText("2026-05-10: Rückfall"));
    expect(screen.getByText(/keine notiz/i)).toBeInTheDocument();
  });

  it("does not show detail for abstinent days", () => {
    const days: CalendarDayDto[] = [{ date: "2026-05-05", status: "abstinent" }];
    render(<CalendarView year={2026} month={5} days={days} />);
    fireEvent.click(screen.getByLabelText("2026-05-05: Abstinent"));
    expect(screen.queryByRole("region", { name: /Rückfälle am/i })).not.toBeInTheDocument();
  });

  it("has a legend with all status types", () => {
    render(<CalendarView year={2026} month={5} days={[]} />);
    expect(screen.getByLabelText("Legende")).toBeInTheDocument();
    expect(screen.getByText(/Abstinent/)).toBeInTheDocument();
    expect(screen.getByText(/Rückfall/)).toBeInTheDocument();
  });

  it("has second visual marker symbol in legend (colour-blind accessibility)", () => {
    render(<CalendarView year={2026} month={5} days={[]} />);
    // Symbols must appear in the legend
    expect(screen.getByText(/✓ Abstinent/)).toBeInTheDocument();
    expect(screen.getByText(/✗ Rückfall/)).toBeInTheDocument();
  });
});
