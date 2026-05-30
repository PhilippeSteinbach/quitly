import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { WeeklyInsightsPage } from "@/features/insights/WeeklyInsightsPage";

vi.mock("@/features/insights/insights.api", () => ({
  useTodayPromptQuery: () => ({ data: { showPrompt: false, message: "" } }),
  useWeeklyInsightQuery: () => ({ data: { checkInCount: 0, abstinentDays: 0, confidence: "low", summaryText: "Insight data will appear soon.", topTriggers: [], moodTrend: {} } }),
  useUpdatePromptPreferenceMutation: () => ({ mutate: vi.fn() })
}));

describe("WeeklyInsightsPage integration shell", () => {
  it("renders empty-state insight content", () => {
    render(
      <MemoryRouter>
        <WeeklyInsightsPage />
      </MemoryRouter>
    );

    expect(screen.getByText(/no prompt is needed right now/i)).toBeInTheDocument();
    expect(screen.getByText(/insight data will appear soon/i)).toBeInTheDocument();
  });
});