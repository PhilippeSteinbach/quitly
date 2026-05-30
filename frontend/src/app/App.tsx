import { useEffect, useRef } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { Toast } from "primereact/toast";
import { Button } from "primereact/button";
import { SkipLink } from "@/components/accessibility/SkipLink";
import { GuestModeBanner } from "@/features/auth/GuestModeBanner";
import { useAuth } from "@/features/auth/useAuth";
import { useCurrentUserQuery } from "@/features/auth/auth.api";
import { useLogoutMutation } from "@/features/auth/auth.api";
import { tokenStorage } from "@/services/httpClient";

export function App() {
  const toastRef = useRef<Toast>(null);
  const navigate = useNavigate();
  const { mode, setMode, setUser, signOut } = useAuth();

  // T041: On mount, hydrate AuthContext from /auth/me when access token exists
  const hasToken = !!tokenStorage.getAccessToken();
  const currentUserQuery = useCurrentUserQuery(hasToken && mode !== "guest");

  useEffect(() => {
    if (currentUserQuery.isSuccess && currentUserQuery.data) {
      setUser({ id: currentUserQuery.data.id, email: currentUserQuery.data.email });
      if (mode !== "authenticated") setMode("authenticated");
    }
    if (currentUserQuery.isError) {
      tokenStorage.clear();
      setMode("unauthenticated");
    }
  }, [currentUserQuery.isSuccess, currentUserQuery.isError, currentUserQuery.data, mode, setMode, setUser]);

  // Wire navigate into AuthContext for session-expired redirect (T014)
  useEffect(() => {
    const handler = () => navigate("/welcome");
    window.addEventListener("auth:session-expired", handler);
    return () => window.removeEventListener("auth:session-expired", handler);
  }, [navigate]);

  // T042: Logout
  const logoutMutation = useLogoutMutation();

  async function handleLogout() {
    try {
      await logoutMutation.mutateAsync();
    } catch {
      // best-effort — always clear local state
    } finally {
      signOut();
      navigate("/welcome");
    }
  }

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Toast ref={toastRef} />
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
            <GuestModeBanner />
            {mode === "authenticated" || mode === "guest" ? (
              <>
                <NavLink className="rounded-full px-4 py-2 hover:bg-white/70" to="/onboarding">
                  Onboarding
                </NavLink>
                <NavLink className="rounded-full px-4 py-2 hover:bg-white/70" to="/check-in">
                  Daily check-in
                </NavLink>
              </>
            ) : null}
            {mode === "authenticated" && (
              <Button
                label="Log out"
                severity="secondary"
                size="small"
                text
                onClick={handleLogout}
              />
            )}
          </nav>
        </header>
        <main className="flex-1" id="main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

