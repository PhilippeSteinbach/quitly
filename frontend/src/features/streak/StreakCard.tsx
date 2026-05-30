import { useEffect, useState } from "react";
import { getNowUtcMs } from "@/lib/streak-calc/monotonic-store";
import { calculateCurrentSeconds } from "@/lib/streak-calc";

interface StreakCardProps {
  /** Server-authoritative streak seconds at last sync. */
  currentStreakSeconds: number;
  /** Server UTC epoch (ms) at last sync — used as monotonic anchor. */
  serverUtcMs: number;
  /** Unix epoch (ms) when the habit started. */
  startedAtMs: number;
}

function formatDuration(totalSeconds: number): { days: number; hours: number; minutes: number; seconds: number } {
  const days    = Math.floor(totalSeconds / 86400);
  const hours   = Math.floor((totalSeconds % 86400) / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  return { days, hours, minutes, seconds };
}

export function StreakCard({ currentStreakSeconds, serverUtcMs, startedAtMs }: StreakCardProps) {
  const [liveSeconds, setLiveSeconds] = useState<number>(currentStreakSeconds);

  useEffect(() => {
    // Recalculate immediately so there's no stale display on mount
    const tick = () => {
      const nowMs = getNowUtcMs();
      const elapsed = Math.max(0, Math.floor((nowMs - serverUtcMs) / 1000));
      setLiveSeconds(currentStreakSeconds + elapsed);
    };

    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [currentStreakSeconds, serverUtcMs, startedAtMs]);

  const { days, hours, minutes, seconds } = formatDuration(liveSeconds);

  return (
    <section
      aria-label="Aktueller Streak"
      className="grid gap-4 rounded-[28px] border border-border bg-white p-6 shadow-soft"
    >
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-slate-500">
        Aktueller Streak
      </p>

      <div className="flex items-end gap-3 flex-wrap" aria-live="polite" aria-atomic="true">
        <span className="text-5xl font-semibold tracking-tight text-emerald-800">
          {days}
        </span>
        <span className="pb-2 text-sm text-slate-600">
          {days === 1 ? "Tag" : "Tage"}
        </span>
        <span className="text-3xl font-medium tracking-tight text-emerald-700">
          {String(hours).padStart(2, "0")}:{String(minutes).padStart(2, "0")}:{String(seconds).padStart(2, "0")}
        </span>
      </div>

      <p className="text-sm leading-6 text-slate-600">
        Sekundengenauer Streak — setzt nach einem Rückfall zurück.
      </p>
    </section>
  );
}

