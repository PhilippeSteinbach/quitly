import { describe, expect, it, vi, afterEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { PostMvpPlaceholder } from "./PostMvpPlaceholder";

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("PostMvpPlaceholder", () => {
  it("renders a gated message when the flag is off", () => {
    vi.stubEnv("VITE_FEATURE_POSTMVP", "");
    render(<PostMvpPlaceholder feature="Achievements" description="x" />);
    expect(screen.getByText(/post-mvp gated/i)).toBeInTheDocument();
  });

  it("renders the preview block when the flag is enabled", () => {
    vi.stubEnv("VITE_FEATURE_POSTMVP", "true");
    render(<PostMvpPlaceholder feature="Achievements" description="preview body" />);
    expect(screen.getByText(/post-mvp preview/i)).toBeInTheDocument();
    expect(screen.getByText(/preview body/i)).toBeInTheDocument();
  });
});
