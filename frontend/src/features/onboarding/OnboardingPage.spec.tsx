import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";
import { OnboardingPage } from "@/features/onboarding/OnboardingPage";

vi.mock("@/features/onboarding/onboarding.api", () => ({
  useActiveHabitQuery: () => ({ data: null }),
  useUpsertHabitMutation: () => ({ isPending: false, isSuccess: false, mutateAsync: vi.fn() })
}));

describe("OnboardingPage", () => {
  it("renders the onboarding form", () => {
    const queryClient = new QueryClient();

    render(
      <MemoryRouter>
        <QueryClientProvider client={queryClient}>
          <OnboardingPage />
        </QueryClientProvider>
      </MemoryRouter>
    );

    expect(screen.getByText(/set one clear habit goal/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /save active goal/i })).toBeInTheDocument();
  });
});
