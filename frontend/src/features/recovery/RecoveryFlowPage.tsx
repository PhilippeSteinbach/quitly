import { useState } from "react";
import { Link } from "react-router-dom";
import { useCreateRelapseMutation, useUpsertRecoveryStepMutation } from "@/features/recovery/recovery.api";

export function RecoveryFlowPage() {
  const [contextNote, setContextNote] = useState("Stress after work");
  const [stepText, setStepText] = useState("Take a ten-minute walk before the next urge.");
  const [relapseId, setRelapseId] = useState<string | null>(null);

  const createRelapseMutation = useCreateRelapseMutation();
  const upsertRecoveryStepMutation = useUpsertRecoveryStepMutation();

  const handleRelapseSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const relapse = await createRelapseMutation.mutateAsync(contextNote);
    setRelapseId(relapse.id);
  };

  const handleRecoverySubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!relapseId) {
      return;
    }

    await upsertRecoveryStepMutation.mutateAsync({
      relapseId,
      stepText,
      completed: true
    });
  };

  return (
    <section className="grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
      <div className="grid gap-5 rounded-[28px] border border-border bg-card p-8 shadow-soft">
        <div className="space-y-2">
          <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-sm text-slate-700">User story 3</span>
          <h2 className="text-3xl font-semibold tracking-tight">A relapse does not erase progress. It defines the next calm step.</h2>
          <p className="max-w-2xl text-base leading-7 text-slate-600">
            The recovery flow stays neutral on purpose: record what happened, choose one next action, keep moving.
          </p>
        </div>

        <form className="grid gap-4" onSubmit={handleRelapseSubmit}>
          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-700">Context note</span>
            <textarea className="min-h-28 rounded-2xl border border-border bg-white px-4 py-3 text-base" maxLength={500} value={contextNote} onChange={(event) => setContextNote(event.target.value)} />
          </label>
          <button className="inline-flex w-fit items-center justify-center rounded-full bg-amber-700 px-5 py-3 text-base font-semibold text-white transition hover:bg-amber-800" type="submit">
            {createRelapseMutation.isPending ? "Recording..." : "Record relapse"}
          </button>
        </form>

        <form className="grid gap-4" onSubmit={handleRecoverySubmit}>
          <label className="grid gap-2">
            <span className="text-sm font-medium text-slate-700">Next step for the next 24 hours</span>
            <input className="rounded-2xl border border-border bg-white px-4 py-3 text-base" maxLength={300} minLength={3} value={stepText} onChange={(event) => setStepText(event.target.value)} />
          </label>
          <button className="inline-flex w-fit items-center justify-center rounded-full bg-emerald-700 px-5 py-3 text-base font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-slate-300" disabled={!relapseId} type="submit">
            {upsertRecoveryStepMutation.isPending ? "Saving step..." : "Complete recovery step"}
          </button>
        </form>
      </div>

      <aside className="grid gap-4 rounded-[28px] border border-border bg-white p-8 shadow-soft">
        <h3 className="text-xl font-semibold">Continuity note</h3>
        <p className="text-sm leading-7 text-slate-600">
          Recovery is framed as continuity, not restart. Previous check-ins remain part of the story, and this screen only asks for one concrete action.
        </p>
        {upsertRecoveryStepMutation.isSuccess ? (
          <p className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            Recovery step completed. You now have a concrete re-entry point for the next day.
          </p>
        ) : null}
        <Link className="text-sm font-medium text-emerald-800 underline-offset-4 hover:underline" to="/check-in">
          Back to daily check-in
        </Link>
      </aside>
    </section>
  );
}
