import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";
import { CheckInPage } from "@/features/checkin/CheckInPage";
import { AuthProvider } from "@/features/auth/AuthContext";

vi.mock("@/features/checkin/checkin.api", () => ({
  useStreakQuery: () => ({ data: { currentStreakDays: 2 } }),
  useCheckInMutation: () => ({ isPending: false, isSuccess: false, mutateAsync: vi.fn() })
}));

vi.mock("@/features/guest/useGuestStreak", () => ({
  useGuestStreakQuery: () => ({ data: { currentStreakDays: 0, longestStreakDays: 0 } })
}));

vi.mock("@/features/guest/useGuestCheckIns", () => ({
  useGuestCheckInMutation: () => ({ isPending: false, isSuccess: false, mutateAsync: vi.fn() })
}));

describe("CheckInPage", () => {
  it("renders the daily check-in form", () => {
    const queryClient = new QueryClient();

    render(
      <MemoryRouter>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>
            <CheckInPage />
          </AuthProvider>
        </QueryClientProvider>
      </MemoryRouter>
    );

    expect(screen.getByText(/log today's status/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /save check-in/i })).toBeInTheDocument();
    expect(screen.getByText(/Tage?/i)).toBeInTheDocument(); // StreakCard shows day unit
  });
});