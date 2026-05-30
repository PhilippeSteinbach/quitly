import { Link } from "react-router-dom";
import { useTodayPromptQuery, useUpdatePromptPreferenceMutation, useWeeklyInsightQuery } from "@/features/insights/insights.api";

export function WeeklyInsightsPage() {
  const promptQuery = useTodayPromptQuery();
  const insightQuery = useWeeklyInsightQuery();
  const updatePreferenceMutation = useUpdatePromptPreferenceMutation();

  return (
    <section className="grid gap-6 lg:grid-cols-[0.95fr_1.05fr]">
      <aside className="grid gap-4 rounded-[28px] border border-border bg-white p-8 shadow-soft">
        <h2 className="text-2xl font-semibold tracking-tight">Passive prompt</h2>
        {promptQuery.data?.showPrompt ? (
          <div className="rounded-2xl bg-amber-50 px-4 py-3 text-sm leading-6 text-amber-900">
            {promptQuery.data.message}
          </div>
        ) : (
          <p className="text-sm leading-6 text-slate-600">No prompt is needed right now.</p>
        )}
        <button
          className="inline-flex w-fit items-center justify-center rounded-full bg-slate-900 px-5 py-3 text-base font-semibold text-white transition hover:bg-slate-700"
          type="button"
          onClick={() => updatePreferenceMutation.mutate({ passivePromptEnabled: false, promptTone: "gentle" })}
        >
          Turn off prompts
        </button>
        <Link className="text-sm font-medium text-emerald-800 underline-offset-4 hover:underline" to="/check-in">
          Back to daily check-in
        </Link>
      </aside>

      <div className="grid gap-4 rounded-[28px] border border-border bg-card p-8 shadow-soft">
        <div className="space-y-2">
          <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-sm text-slate-700">User story 5</span>
          <h2 className="text-3xl font-semibold tracking-tight">Weekly patterns in plain language.</h2>
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <MetricCard label="Check-ins" value={String(insightQuery.data?.checkInCount ?? 0)} />
          <MetricCard label="Abstinent days" value={String(insightQuery.data?.abstinentDays ?? 0)} />
          <MetricCard label="Confidence" value={insightQuery.data?.confidence ?? "low"} />
        </div>

        <p className="rounded-2xl bg-white px-5 py-4 text-sm leading-7 text-slate-700">
          {insightQuery.data?.summaryText ?? "Insight data will appear here once weekly aggregation is available."}
        </p>

        <div className="grid gap-2">
          <h3 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-500">Top triggers</h3>
          <div className="flex flex-wrap gap-2">
            {(insightQuery.data?.topTriggers ?? []).map((trigger) => (
              <span key={trigger} className="rounded-full bg-white px-3 py-1 text-sm text-slate-700">
                {trigger.replaceAll("_", " ")}
              </span>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-border bg-white px-4 py-5">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold tracking-tight text-slate-900">{value}</p>
    </div>
  );
}
