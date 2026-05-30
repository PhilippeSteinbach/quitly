const FEATURE_FLAG_ENV = "VITE_FEATURE_POSTMVP" as const;

export function isPostMvpEnabled(): boolean {
  const value = import.meta.env[FEATURE_FLAG_ENV];
  return value === "true" || value === "1";
}

type PostMvpPlaceholderProps = {
  feature: string;
  description: string;
};

export function PostMvpPlaceholder({ feature, description }: PostMvpPlaceholderProps) {
  if (!isPostMvpEnabled()) {
    return (
      <section
        role="status"
        aria-live="polite"
        className="grid gap-3 rounded-[28px] border border-dashed border-border bg-muted/40 p-8 text-slate-600"
      >
        <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-xs font-medium uppercase tracking-wide">
          Post-MVP gated
        </span>
        <h2 className="text-xl font-semibold">{feature}</h2>
        <p className="max-w-xl text-sm leading-6">
          This experience is intentionally disabled in the MVP scope. Enable {FEATURE_FLAG_ENV}={`"true"`} to preview it.
        </p>
      </section>
    );
  }

  return (
    <section className="grid gap-4 rounded-[28px] border border-border bg-card p-8 shadow-soft">
      <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-xs font-medium uppercase tracking-wide">
        Post-MVP preview
      </span>
      <h2 className="text-2xl font-semibold">{feature}</h2>
      <p className="max-w-2xl text-sm leading-6 text-slate-600">{description}</p>
    </section>
  );
}
