import type { StreakDto } from "@/features/checkin/checkin.api";

type StreakCardProps = {
  streak?: StreakDto;
};

export function StreakCard({ streak }: StreakCardProps) {
  return (
    <section className="grid gap-4 rounded-[28px] border border-border bg-white p-6 shadow-soft">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-slate-500">Current streak</p>
      <div className="flex items-end gap-3">
        <span className="text-5xl font-semibold tracking-tight text-emerald-800">
          {streak?.currentStreakDays ?? 0}
        </span>
        <span className="pb-2 text-sm text-slate-600">abstinent days</span>
      </div>
      <p className="text-sm leading-6 text-slate-600">
        Strict streak logic resets after a non-abstinent day and pauses when the abstinent evidence chain breaks.
      </p>
    </section>
  );
}
