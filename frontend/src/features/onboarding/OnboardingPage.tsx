import { useState } from "react";
import { useActiveHabitQuery, useUpsertHabitMutation, type HabitCategory, type HabitMode } from "@/features/onboarding/onboarding.api";
import { useGuestHabitQuery, useUpsertGuestHabitMutation } from "@/features/guest/useGuestHabit";
import { useAuth } from "@/features/auth/useAuth";

const categories: Array<{ value: HabitCategory; label: string }> = [
  { value: "smoking", label: "Smoking" },
  { value: "social_media", label: "Social media" },
  { value: "sugar", label: "Sugar" },
  { value: "impulse_buying", label: "Impulse buying" },
  { value: "custom", label: "Custom" }
];

export function OnboardingPage() {
  const [category, setCategory] = useState<HabitCategory>("smoking");
  const [mode, setMode] = useState<HabitMode>("quit");
  const [title, setTitle] = useState("Quit smoking after dinner");

  const { mode: authMode } = useAuth();
  const isGuest = authMode === "guest";

  // Authenticated path
  const activeHabitQuery = useActiveHabitQuery();
  const upsertHabitMutation = useUpsertHabitMutation();

  // Guest path
  const guestHabitQuery = useGuestHabitQuery();
  const upsertGuestHabitMutation = useUpsertGuestHabitMutation();

  const activeHabit = isGuest ? guestHabitQuery.data : activeHabitQuery.data;
  const isPending = isGuest ? upsertGuestHabitMutation.isPending : upsertHabitMutation.isPending;
  const isSuccess = isGuest ? upsertGuestHabitMutation.isSuccess : upsertHabitMutation.isSuccess;

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (isGuest) {
      await upsertGuestHabitMutation.mutateAsync({
        title,
        category,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        createdAt: new Date().toISOString()
      });
    } else {
      await upsertHabitMutation.mutateAsync({
        category,
        mode,
        title,
        startedOn: new Date().toISOString().slice(0, 10)
      });
    }
  };

  return (
    <section className="grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
      <form className="grid gap-5 rounded-[28px] border border-border bg-card p-8 shadow-soft" onSubmit={handleSubmit}>
        <div className="space-y-2">
          <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-sm text-slate-700">
            User story 1
          </span>
          <h2 className="text-3xl font-semibold tracking-tight">Set one clear habit goal and start fast.</h2>
          <p className="max-w-2xl text-base leading-7 text-slate-600">
            Quitly keeps the first step narrow: one primary habit, one mode, one title you can recognize every day.
          </p>
        </div>

        <label className="grid gap-2">
          <span className="text-sm font-medium text-slate-700">Habit category</span>
          <select className="rounded-2xl border border-border bg-white px-4 py-3 text-base" value={category} onChange={(event) => setCategory(event.target.value as HabitCategory)}>
            {categories.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </label>

        <fieldset className="grid gap-3">
          <legend className="text-sm font-medium text-slate-700">Goal mode</legend>
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="rounded-2xl border border-border bg-white px-4 py-3">
              <input checked={mode === "quit"} className="mr-3" name="mode" type="radio" value="quit" onChange={() => setMode("quit")} />
              Quit entirely
            </label>
            <label className="rounded-2xl border border-border bg-white px-4 py-3">
              <input checked={mode === "reduce"} className="mr-3" name="mode" type="radio" value="reduce" onChange={() => setMode("reduce")} />
              Reduce steadily
            </label>
          </div>
        </fieldset>

        <label className="grid gap-2">
          <span className="text-sm font-medium text-slate-700">Goal title</span>
          <input className="rounded-2xl border border-border bg-white px-4 py-3 text-base" maxLength={120} minLength={2} value={title} onChange={(event) => setTitle(event.target.value)} />
        </label>

        <button className="inline-flex w-fit items-center justify-center rounded-full bg-emerald-700 px-5 py-3 text-base font-semibold text-white transition hover:bg-emerald-800" type="submit">
          {isPending ? "Saving..." : "Save active goal"}
        </button>

        {isSuccess ? (
          <p className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            Active goal saved. You can continue into daily check-ins next.
          </p>
        ) : null}
      </form>

      <aside className="grid gap-4 rounded-[28px] border border-border bg-white/90 p-8 shadow-soft">
        <h3 className="text-xl font-semibold">Active goal snapshot</h3>
        {activeHabit ? (
          <dl className="grid gap-3 text-sm text-slate-700">
            <div>
              <dt className="font-semibold">Title</dt>
              <dd>{activeHabit.title}</dd>
            </div>
            <div>
              <dt className="font-semibold">Mode</dt>
              <dd>{"mode" in activeHabit ? activeHabit.mode : "—"}</dd>
            </div>
            <div>
              <dt className="font-semibold">Category</dt>
              <dd>{activeHabit.category}</dd>
            </div>
          </dl>
        ) : (
          <p className="text-sm leading-7 text-slate-600">
            No active habit loaded yet. After authentication is wired in the browser, this panel will reflect the saved habit from the API.
          </p>
        )}
      </aside>
    </section>
  );
}
