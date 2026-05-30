import type { MonthStatsDto } from "@/services/streak.api";

const MONTH_NAMES = [
  "Januar", "Februar", "März", "April", "Mai", "Juni",
  "Juli", "August", "September", "Oktober", "November", "Dezember",
];

interface MonthStatsProps {
  stats: MonthStatsDto;
}

export function MonthStats({ stats }: MonthStatsProps) {
  const monthName = MONTH_NAMES[stats.month - 1] ?? `Monat ${stats.month}`;

  return (
    <section
      aria-label={`Monatsstatistik ${monthName} ${stats.year}`}
      className="rounded-2xl border border-border bg-white p-4 shadow-soft"
    >
      <div className="flex items-center gap-2 mb-2">
        <h3 className="text-sm font-semibold text-slate-700">
          {monthName} {stats.year}
        </h3>
        {stats.isCurrentMonth && (
          <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">
            Laufend
          </span>
        )}
      </div>

      <p className="text-2xl font-semibold text-emerald-800">
        {stats.abstinentDays}
        <span className="text-base font-normal text-slate-500">
          {" "}von {stats.relevantDays} Tagen abstinent
        </span>
      </p>

      {stats.relapseCount > 0 && (
        <p className="mt-1 text-xs text-slate-500">
          {stats.relapseCount} {stats.relapseCount === 1 ? "Rückfall" : "Rückfälle"}
        </p>
      )}
    </section>
  );
}
