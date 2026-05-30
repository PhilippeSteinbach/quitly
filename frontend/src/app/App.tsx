import { NavLink, Outlet } from "react-router-dom";
import { SkipLink } from "@/components/accessibility/SkipLink";

export function App() {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <SkipLink />
      <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="mb-8 flex items-center justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.3em] text-slate-500">
              Quitly
            </p>
            <h1 className="mt-2 text-3xl font-semibold tracking-tight">
              Sustainable habit reduction, one clear next step at a time.
            </h1>
          </div>
          <nav className="flex items-center gap-3 text-sm font-medium text-slate-600">
            <NavLink className="rounded-full px-4 py-2 hover:bg-white/70" to="/">
              Onboarding
            </NavLink>
            <NavLink className="rounded-full px-4 py-2 hover:bg-white/70" to="/check-in">
              Daily check-in
            </NavLink>
          </nav>
        </header>
        <main className="flex-1" id="main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
