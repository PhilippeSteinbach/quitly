import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { FloatLabel } from "primereact/floatlabel";
import { Message } from "primereact/message";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { useRegisterMutation } from "@/features/auth/auth.api";
import { useAuth } from "@/features/auth/useAuth";
import { clearGuestData, getGuestHabit } from "@/features/guest/guestStorage";
import { downloadGuestExport } from "@/features/guest/guestExport";

export function RegisterPage() {
  const navigate = useNavigate();
  const { mode, setMode, setUser } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const toastRef = useRef<{ show: (opts: unknown) => void } | null>(null);

  const registerMutation = useRegisterMutation();

  const hasGuestData = mode === "guest" && getGuestHabit() !== null;

  const handleSubmit = useCallback(
    async (event: React.FormEvent<HTMLFormElement>) => {
      event.preventDefault();
      setEmailError(null);

      const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;

      try {
        const data = await registerMutation.mutateAsync({ email, password, timezone });

        // T044b: show ConfirmDialog before clearing guest data
        if (hasGuestData) {
          confirmDialog({
            message: "Your local tracking data will be cleared from this device. This cannot be undone.",
            header: "Clear local data?",
            acceptLabel: "Continue",
            rejectLabel: "Cancel",
            accept() {
              clearGuestData(); // T045
              setUser({ id: "", email }); // will be overwritten by /auth/me query
              setMode("authenticated");
              navigate("/onboarding");
            },
            reject() {
              // User cancelled — stay on register page, tokens already stored
              // Data kept intact, mode stays as-is until explicit confirm
            }
          });
        } else {
          setUser({ id: "", email });
          setMode("authenticated");
          navigate("/onboarding");
        }
        void data;
      } catch (err: unknown) {
        const axiosError = err as { response?: { data?: { error?: string } } };
        if (axiosError?.response?.data?.error === "email_taken") {
          setEmailError("This email is already registered.");
        }
      }
    },
    [email, password, hasGuestData, navigate, registerMutation, setMode, setUser]
  );

  const header = (
    <div className="p-6 pb-0">
      <h2 className="text-2xl font-semibold tracking-tight text-slate-900">Create account</h2>
      <p className="mt-1 text-sm text-slate-500">Start tracking your progress.</p>
    </div>
  );

  return (
    <div className="flex min-h-[70vh] items-center justify-center">
      {/* T044b: ConfirmDialog must be mounted in the tree */}
      <ConfirmDialog />

      <Card className="w-full max-w-md" header={header}>
        {/* T044: Guest data export banner */}
        {hasGuestData && (
          <div className="mb-4">
            <Message
              className="w-full"
              severity="warn"
              content={
                <div className="flex flex-wrap items-center gap-3">
                  <span>You have local guest data. Download a backup before registering.</span>
                  <Button
                    label="Download JSON backup"
                    severity="warning"
                    size="small"
                    text
                    onClick={() => downloadGuestExport()}
                  />
                </div>
              }
            />
          </div>
        )}

        <form className="grid gap-6" onSubmit={handleSubmit}>
          {registerMutation.isError && !emailError && (
            <Message
              className="w-full"
              severity="error"
              text="Registration failed. Please try again."
            />
          )}

          <div className="grid gap-2">
            <FloatLabel>
              <InputText
                autoComplete="email"
                className={`w-full ${emailError ? "p-invalid" : ""}`}
                id="reg-email"
                required
                type="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setEmailError(null);
                }}
              />
              <label htmlFor="reg-email">Email</label>
            </FloatLabel>
            {emailError && (
              <Message className="w-full" severity="error" text={emailError} />
            )}
          </div>

          <FloatLabel>
            <Password
              className="w-full"
              feedback
              inputId="reg-password"
              inputStyle={{ width: "100%" }}
              required
              toggleMask
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <label htmlFor="reg-password">Password</label>
          </FloatLabel>

          <Button
            className="w-full"
            disabled={registerMutation.isPending}
            label={registerMutation.isPending ? "Creating account…" : "Create account"}
            type="submit"
          />

          <p className="text-center text-sm text-slate-600">
            Already have an account?{" "}
            <button
              className="font-medium underline-offset-4 hover:underline"
              type="button"
              onClick={() => navigate("/login")}
            >
              Log in
            </button>
          </p>
        </form>
      </Card>
    </div>
  );
}
