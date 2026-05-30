import { afterEach, describe, expect, it, vi } from "vitest";
import { buildGuestExport } from "@/features/guest/guestExport";
import { saveGuestHabit, upsertGuestCheckIn } from "@/features/guest/guestStorage";
import type { GuestExport } from "@/features/guest/guestExport";

afterEach(() => {
  localStorage.clear();
  vi.restoreAllMocks();
});

describe("buildGuestExport", () => {
  it("schema shape: all required fields present", () => {
    const result = buildGuestExport();
    const keys: Array<keyof GuestExport> = ["formatVersion", "exportedAt", "habit", "checkIns"];
    for (const key of keys) {
      expect(result).toHaveProperty(key);
    }
  });

  it("formatVersion is always '1'", () => {
    const result = buildGuestExport();
    expect(result.formatVersion).toBe("1");
  });

  it("exportedAt is a valid ISO UTC timestamp", () => {
    const result = buildGuestExport();
    const parsed = new Date(result.exportedAt);
    expect(Number.isNaN(parsed.getTime())).toBe(false);
    expect(result.exportedAt).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/);
  });

  it("null habit when no habit stored", () => {
    const result = buildGuestExport();
    expect(result.habit).toBeNull();
    expect(result.checkIns).toEqual([]);
  });

  it("includes stored habit when present", () => {
    saveGuestHabit({ title: "Quit", category: "smoking", timezone: "UTC", createdAt: "2025-01-01T00:00:00Z" });
    const result = buildGuestExport();
    expect(result.habit?.title).toBe("Quit");
  });

  it("includes stored check-ins", () => {
    upsertGuestCheckIn({ date: "2025-01-10", status: "abstinent", mood: "neutral", triggers: [], note: "" });
    const result = buildGuestExport();
    expect(result.checkIns).toHaveLength(1);
    expect(result.checkIns[0].date).toBe("2025-01-10");
  });
});

describe("downloadGuestExport filename", () => {
  it("filename contains today's date in YYYY-MM-DD format", async () => {
    const mockToday = "2025-06-15";
    vi.spyOn(Date.prototype, "toISOString").mockReturnValue(`${mockToday}T12:00:00.000Z`);

    // Capture the anchor element to check the filename
    const anchors: HTMLAnchorElement[] = [];
    const origCreate = document.createElement.bind(document);
    vi.spyOn(document, "createElement").mockImplementation((tag: string) => {
      const el = origCreate(tag);
      if (tag === "a") anchors.push(el as HTMLAnchorElement);
      return el;
    });

    // Mock URL.createObjectURL since jsdom doesn't support it
    vi.stubGlobal("URL", {
      createObjectURL: vi.fn(() => "blob:mock"),
      revokeObjectURL: vi.fn()
    });

    const { downloadGuestExport } = await import("@/features/guest/guestExport");
    downloadGuestExport();

    expect(anchors[0]?.download).toContain(mockToday);
  });
});
