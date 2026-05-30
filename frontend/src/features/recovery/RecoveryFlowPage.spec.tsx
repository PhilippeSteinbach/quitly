import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { RecoveryFlowPage } from "@/features/recovery/RecoveryFlowPage";

vi.mock("@/features/recovery/recovery.api", () => ({
  useCreateRelapseMutation: () => ({ isPending: false, mutateAsync: vi.fn(async () => ({ id: "relapse-1" })) }),
  useUpsertRecoveryStepMutation: () => ({ isPending: false, isSuccess: false, mutateAsync: vi.fn() })
}));

describe("RecoveryFlowPage", () => {
  it("renders the relapse and recovery forms", () => {
    render(
      <MemoryRouter>
        <RecoveryFlowPage />
      </MemoryRouter>
    );

    expect(screen.getByText(/a relapse does not erase progress/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /record relapse/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /complete recovery step/i })).toBeDisabled();
  });
});
