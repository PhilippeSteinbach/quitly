import type { MonthStatsDto } from "@/services/streak.api";

interface YearHeatmapProps {
  year: number;
  months: MonthStatsDto[];
  /** ISO date string (YYYY-MM-DD) of habit start — months before this are neutral. */
  startedOnDate: string;
}

const MONTH_ABBR = ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];

function absRate(m: MonthStatsDto): number {
  if (m.relevantDays === 0) return 0;
  return m.abstinentDays / m.relevantDays;
}

/**
 * Returns a Tailwind opacity class for the heatmap tile based on abstinence rate.
 * 0 % → lightest; 100 % → darkest green.
 */
function opacityClass(rate: number): string {
  if (rate >= 0.9) return "bg-emerald-600";
  if (rate >= 0.7) return "bg-emerald-400";
  if (rate >= 0.5) return "bg-emerald-300";
  if (rate >= 0.25) return "bg-emerald-200";
  if (rate > 0)    return "bg-emerald-100";
  return "bg-red-100";
}

export function YearHeatmap({ year, months, startedOnDate }: YearHeatmapProps) {
  const startedYear  = parseInt(startedOnDate.slice(0, 4), 10);
  const startedMonth = parseInt(startedOnDate.slice(5, 7), 10);
  const isBeforeStart = (monthIndex: number) =>
    year < startedYear || (year === startedYear && monthIndex + 1 < startedMonth);

  return (
    <section aria-label={`Jahresübersicht ${year}`} className="space-y-2">
      <h2 className="text-sm font-semibold text-slate-700">{year}</h2>

      <div className="grid grid-cols-6 gap-2 sm:grid-cols-12">
        {months.map((m, i) => {
          const neutral = isBeforeStart(i) || m.relevantDays === 0;
          const rate    = absRate(m);
          const abbr    = MONTH_ABBR[i];

          if (neutral) {
            return (
              <div
                key={i}
                aria-label={`${abbr}: Neutral`}
                className="flex h-10 flex-col items-center justify-center rounded-lg bg-slate-100 text-slate-300"
              >
                <span className="text-[10px]">{abbr}</span>
              </div>
            );
          }

          return (
            <div
              key={i}
              role="img"
              aria-label={`${abbr}: ${Math.round(rate * 100)} % abstinent`}
              title={`${abbr} ${year}: ${m.abstinentDays}/${m.relevantDays} Tage`}
              className={`flex h-10 flex-col items-center justify-center rounded-lg ${opacityClass(rate)} transition-colors`}
            >
              <span className="text-[10px] font-medium text-white drop-shadow">{abbr}</span>
              <span className="text-[9px] text-white/80">{Math.round(rate * 100)} %</span>
            </div>
          );
        })}
      </div>
    </section>
  );
}
