import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { StreakCard } from "@/features/streak/StreakCard";

describe("StreakCard", () => {
  it("renders the streak count", () => {
    render(<StreakCard streak={{ currentStreakDays: 4 }} />);

    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText(/abstinent days/i)).toBeInTheDocument();
  });
});
