import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ConsentGate } from "@/features/privacy/ConsentGate";

describe("ConsentGate", () => {
  it("does not render analytics consent UI in MVP mode", () => {
    render(
      <ConsentGate>
        <div>dashboard content</div>
      </ConsentGate>
    );

    expect(screen.getByText("dashboard content")).toBeInTheDocument();
    expect(screen.queryByText(/analytics consent/i)).not.toBeInTheDocument();
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });
});