import { useState } from "react";
import type { CalendarDayDto } from "@/services/streak.api";

interface CalendarViewProps {
  year: number;
  month: number;
  days: CalendarDayDto[];
}

const WEEKDAYS = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];
const MONTH_NAMES = [
  "Januar", "Februar", "März", "April", "Mai", "Juni",
  "Juli", "August", "September", "Oktober", "November", "Dezember",
];

// Second visual marker for colour-blind accessibility (WCAG SC 1.4.1)
const STATUS_CONFIG = {
  abstinent: {
    bg: "bg-emerald-100",
    text: "text-emerald-800",
    label: "Abstinent",
    symbol: "✓",          // second visual marker: check symbol
    pattern: "pattern-abstinent",
  },
  relapse: {
    bg: "bg-red-100",
    text: "text-red-800",
    label: "Rückfall",
    symbol: "✗",           // second visual marker: cross symbol
    pattern: "pattern-relapse",
  },
  paused: {
    bg: "bg-amber-50",
    text: "text-amber-700",
    label: "Pausiert",
    symbol: "⏸",
    pattern: "pattern-paused",
  },
  neutral: {
    bg: "bg-slate-50",
    text: "text-slate-400",
    label: "Neutral",
    symbol: "·",
    pattern: "pattern-neutral",
  },
} as const;

export function CalendarView({ year, month, days }: CalendarViewProps) {
  const [selectedDate, setSelectedDate] = useState<string | null>(null);

  const monthName = MONTH_NAMES[month - 1];
  const selectedDay = selectedDate ? days.find(d => d.date === selectedDate) : null;

  // Compute offset: which weekday does the 1st fall on? (0=Mo … 6=So)
  const firstDate = new Date(year, month - 1, 1);
  const startOffset = (firstDate.getDay() + 6) % 7; // convert Sunday=0 to Mon-based

  return (
    <section aria-label={`Kalender ${monthName} ${year}`} className="space-y-3">
      <h2 className="text-sm font-semibold text-slate-700">{monthName} {year}</h2>

      {/* Weekday headers */}
      <div className="grid grid-cols-7 gap-1">
        {WEEKDAYS.map(d => (
          <div key={d} className="text-center text-xs font-medium text-slate-400">{d}</div>
        ))}

        {/* Blank cells for offset */}
        {Array.from({ length: startOffset }).map((_, i) => (
          <div key={`blank-${i}`} />
        ))}

        {/* Day cells */}
        {days.map(day => {
          const config = STATUS_CONFIG[day.status] ?? STATUS_CONFIG.neutral;
          const dayNum = parseInt(day.date.slice(8, 10), 10);
          const isSelected = day.date === selectedDate;

          return (
            <button
              key={day.date}
              type="button"
              aria-label={`${day.date}: ${config.label}`}
              aria-pressed={isSelected}
              onClick={() => setSelectedDate(isSelected ? null : day.date)}
              className={`
                relative flex h-9 w-full items-center justify-center rounded-lg text-xs font-medium
                transition-all focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2
                focus-visible:outline-emerald-600
                ${config.bg} ${config.text}
                ${isSelected ? "ring-2 ring-emerald-500" : ""}
              `}
            >
              <span className="sr-only">{config.symbol} </span>
              {dayNum}
              {/* Second visual marker: small symbol in corner for colour-blind users */}
              <span
                aria-hidden="true"
                className="absolute bottom-0.5 right-1 text-[8px] leading-none opacity-70"
              >
                {day.status !== "neutral" ? config.symbol : ""}
              </span>
            </button>
          );
        })}
      </div>

      {/* Relapse detail sheet */}
      {selectedDay && selectedDay.status === "relapse" && (
        <div
          role="region"
          aria-label={`Rückfälle am ${selectedDay.date}`}
          className="rounded-xl border border-red-200 bg-red-50 p-3 space-y-2"
        >
          <h3 className="text-xs font-semibold text-red-700">
            Rückfälle am {selectedDay.date}
          </h3>
          {selectedDay.notes && selectedDay.notes.length > 0 ? (
            <ul className="space-y-1">
              {selectedDay.notes.map((note, i) => (
                <li key={i} className="text-xs text-slate-700">
                  {note}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-xs text-slate-500">Keine Notiz hinterlassen.</p>
          )}
        </div>
      )}

      {/* Legend */}
      <div className="flex flex-wrap gap-3 pt-1" aria-label="Legende">
        {(Object.entries(STATUS_CONFIG) as [keyof typeof STATUS_CONFIG, typeof STATUS_CONFIG[keyof typeof STATUS_CONFIG]][]).map(([status, cfg]) => (
          <div key={status} className="flex items-center gap-1">
            <span className={`inline-block h-3 w-3 rounded-sm ${cfg.bg}`} aria-hidden="true" />
            <span className="text-[10px] text-slate-500">{cfg.symbol} {cfg.label}</span>
          </div>
        ))}
      </div>
    </section>
  );
}
