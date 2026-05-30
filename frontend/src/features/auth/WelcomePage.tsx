import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { Divider } from "primereact/divider";
import { useAuth } from "@/features/auth/useAuth";

export function WelcomePage() {
  const navigate = useNavigate();
  const { mode, setMode } = useAuth();

  // T028: inverse redirect — authenticated users should not see this page
  if (mode === "authenticated") {
    navigate("/onboarding", { replace: true });
    return null;
  }

  function handleContinueAsGuest() {
    setMode("guest");
    navigate("/onboarding");
  }

  const header = (
    <div className="p-6 pb-0">
      <p className="text-sm font-semibold uppercase tracking-[0.3em] text-slate-500">Quitly</p>
      <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-900">
        Sustainable habit reduction, one clear next step at a time.
      </h1>
    </div>
  );

  return (
    <div className="flex min-h-[70vh] items-center justify-center">
      <Card className="w-full max-w-md" header={header}>
        <p className="mb-6 text-base leading-7 text-slate-600">
          Track your progress privately — no account required to get started.
        </p>

        <div className="grid gap-3">
          <Button
            className="w-full"
            label="Log in"
            severity="secondary"
            onClick={() => navigate("/login")}
          />
          <Button
            className="w-full"
            label="Create account"
            onClick={() => navigate("/register")}
          />
        </div>

        <Divider align="center">
          <span className="text-sm text-slate-500">or</span>
        </Divider>

        <Button
          className="w-full"
          label="Continue as Guest"
          outlined
          severity="secondary"
          onClick={handleContinueAsGuest}
        />

        <p className="mt-4 text-center text-xs text-slate-500">
          Guest mode stores your data locally on this device only.
        </p>
      </Card>
    </div>
  );
}
