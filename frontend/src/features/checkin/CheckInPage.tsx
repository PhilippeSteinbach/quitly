import { useState } from "react";
import { Link } from "react-router-dom";
import { Message } from "primereact/message";
import { useCheckInMutation, useStreakQuery, type CheckInStatus, type MoodValue } from "@/features/checkin/checkin.api";
import { StreakCard } from "@/features/streak/StreakCard";
import { useAuth } from "@/features/auth/useAuth";
import { useGuestCheckInMutation } from "@/features/guest/useGuestCheckIns";
import { useGuestStreakQuery } from "@/features/guest/useGuestStreak";
import { isNoticeDismissed, dismissNotice } from "@/features/guest/guestStorage";

const triggerOptions = ["stress", "boredom", "social", "late_night"] as const;

export function CheckInPage() {
  const [status, setStatus] = useState<CheckInStatus>("abstinent");
  const [mood, setMood] = useState<MoodValue>("neutral");
  const [selectedTriggers, setSelectedTriggers] = useState<string[]>([]);
  const [note, setNote] = useState("");
  const [noticeDismissed, setNoticeDismissed] = useState(isNoticeDismissed);

  const { mode: authMode } = useAuth();
  const isGuest = authMode === "guest";

  // Authenticated hooks
  const streakQuery = useStreakQuery();
  const checkInMutation = useCheckInMutation();

  // Guest hooks
  const guestStreakQuery = useGuestStreakQuery();
  const guestCheckInMutation = useGuestCheckInMutation();

  const isPending = isGuest ? guestCheckInMutation.isPending : checkInMutation.isPending;
  const isSuccess = isGuest ? guestCheckInMutation.isSuccess : checkInMutation.isSuccess;
  const streakData = isGuest ? guestStreakQuery.data : streakQuery.data;

  const toggleTrigger = (value: string) => {
    setSelectedTriggers((current) =>
      current.includes(value) ? current.filter((item) => item !== value) : [...current, value]
    );
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (isGuest) {
      const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
      const today = new Date().toLocaleDateString("en-CA", { timeZone: timezone });
      await guestCheckInMutation.mutateAsync({
        date: today,
        status,
        mood: mood as "very_low" | "low" | "neutral" | "good" | "very_good",
        triggers: selectedTriggers,
        note
      });
    } else {
      await checkInMutation.mutateAsync({
        day: new Date().toISOString().slice(0, 10),
        status,
        mood,
        triggers: selectedTriggers,
        note
      });
    }
  };

  return (
    <section className="grid gap-6 lg:grid-cols-[1.15fr_0.85fr]">
      <form className="grid gap-5 rounded-[28px] border border-border bg-card p-8 shadow-soft" onSubmit={handleSubmit}>
        {/* T035: passive notice for guest mode — shown only first time */}
        {isGuest && !noticeDismissed && (
          <Message
            className="w-full"
            severity="info"
            closable
            text="Your data is stored locally on this device only."
            onClose={() => {
              dismissNotice();
              setNoticeDismissed(true);
            }}
          />
        )}
        <div className="space-y-2">
          <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-sm text-slate-700">User story 2</span>
          <h2 className="text-3xl font-semibold tracking-tight">Log today&apos;s status, mood, and triggers in one short pass.</h2>
          <p className="max-w-2xl text-base leading-7 text-slate-600">
            The check-in stays intentionally short so it can survive stressful, low-attention moments.
          </p>
        </div>

        <fieldset className="grid gap-3">
          <legend className="text-sm font-medium text-slate-700">Status</legend>
          <div className="grid gap-3 sm:grid-cols-3">
            {[
              { value: "abstinent", label: "Abstinent" },
              { value: "non_abstinent", label: "Non-abstinent" },
              { value: "unsure", label: "Unsure" }
            ].map((option) => (
              <label key={option.value} className="rounded-2xl border border-border bg-white px-4 py-3">
                <input checked={status === option.value} className="mr-3" name="status" type="radio" value={option.value} onChange={() => setStatus(option.value as CheckInStatus)} />
                {option.label}
              </label>
            ))}
          </div>
        </fieldset>

        <label className="grid gap-2">
          <span className="text-sm font-medium text-slate-700">Mood</span>
          <select className="rounded-2xl border border-border bg-white px-4 py-3 text-base" value={mood} onChange={(event) => setMood(event.target.value as MoodValue)}>
            <option value="very_low">Very low</option>
            <option value="low">Low</option>
            <option value="neutral">Neutral</option>
            <option value="good">Good</option>
            <option value="very_good">Very good</option>
          </select>
        </label>

        <fieldset className="grid gap-3">
          <legend className="text-sm font-medium text-slate-700">Triggers</legend>
          <div className="flex flex-wrap gap-3">
            {triggerOptions.map((option) => (
              <button key={option} className={`rounded-full border px-4 py-2 text-sm ${selectedTriggers.includes(option) ? "border-emerald-700 bg-emerald-50 text-emerald-900" : "border-border bg-white text-slate-700"}`} type="button" onClick={() => toggleTrigger(option)}>
                {option.replace("_", " ")}
              </button>
            ))}
          </div>
        </fieldset>

        <label className="grid gap-2">
          <span className="text-sm font-medium text-slate-700">Note</span>
          <textarea className="min-h-32 rounded-2xl border border-border bg-white px-4 py-3 text-base" maxLength={500} value={note} onChange={(event) => setNote(event.target.value)} />
        </label>

        <div className="flex flex-wrap items-center gap-3">
          <button className="inline-flex w-fit items-center justify-center rounded-full bg-emerald-700 px-5 py-3 text-base font-semibold text-white transition hover:bg-emerald-800" type="submit">
            {isPending ? "Saving..." : "Save check-in"}
          </button>
          <Link className="text-sm font-medium text-emerald-800 underline-offset-4 hover:underline" to="/onboarding">
            Back to onboarding
          </Link>
        </div>

        {isSuccess ? (
          <p className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            Check-in saved. The streak snapshot refreshes from the API on the next successful query.
          </p>
        ) : null}
      </form>

      <StreakCard
        currentStreakSeconds={(streakData && "currentStreakDays" in streakData ? streakData.currentStreakDays : 0) * 86400}
        serverUtcMs={Date.now()}
        startedAtMs={Date.now() - (streakData && "currentStreakDays" in streakData ? streakData.currentStreakDays : 0) * 86400_000}
      />
    </section>
  );
}
