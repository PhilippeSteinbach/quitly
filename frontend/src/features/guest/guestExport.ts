import { getGuestCheckIns, getGuestHabit } from "@/features/guest/guestStorage";
import type { GuestCheckIn, GuestHabit } from "@/features/guest/guestStorage";

export type GuestExport = {
  formatVersion: "1";
  exportedAt: string; // ISO UTC timestamp
  habit: GuestHabit | null;
  checkIns: GuestCheckIn[];
};

export function buildGuestExport(): GuestExport {
  return {
    formatVersion: "1",
    exportedAt: new Date().toISOString(),
    habit: getGuestHabit(),
    checkIns: getGuestCheckIns()
  };
}

export function downloadGuestExport(): void {
  const data = buildGuestExport();
  const json = JSON.stringify(data, null, 2);
  const blob = new Blob([json], { type: "application/json" });
  const url = URL.createObjectURL(blob);

  const today = new Date().toISOString().slice(0, 10); // YYYY-MM-DD
  const filename = `quitly-backup-${today}.json`;

  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  anchor.style.display = "none";
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);

  // Release object URL after a short delay to allow the download to start
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
