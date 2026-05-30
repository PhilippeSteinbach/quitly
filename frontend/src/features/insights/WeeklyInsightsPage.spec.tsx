import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { WeeklyInsightsPage } from "@/features/insights/WeeklyInsightsPage";

vi.mock("@/features/insights/insights.api", () => ({
  useTodayPromptQuery: () => ({ data: { showPrompt: true, message: "A short check-in is enough for today." } }),
  useWeeklyInsightQuery: () => ({ data: { checkInCount: 4, abstinentDays: 3, confidence: "medium", summaryText: "You logged 4 check-ins.", topTriggers: ["stress"], moodTrend: {} } }),
  useUpdatePromptPreferenceMutation: () => ({ mutate: vi.fn() })
}));

describe("WeeklyInsightsPage", () => {
  it("renders prompt and insight content", () => {
    render(
      <MemoryRouter>
        <WeeklyInsightsPage />
      </MemoryRouter>
    );

    expect(screen.getByText(/weekly patterns in plain language/i)).toBeInTheDocument();
    expect(screen.getByText(/a short check-in is enough for today/i)).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
  });
});
