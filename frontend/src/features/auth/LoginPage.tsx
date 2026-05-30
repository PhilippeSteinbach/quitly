import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { FloatLabel } from "primereact/floatlabel";
import { Message } from "primereact/message";
import { useLoginMutation } from "@/features/auth/auth.api";
import { useAuth } from "@/features/auth/useAuth";

export function LoginPage() {
  const navigate = useNavigate();
  const { setMode } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const loginMutation = useLoginMutation();

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      await loginMutation.mutateAsync({ email, password });
      setMode("authenticated");
      navigate("/onboarding");
    } catch {
      // Error shown via loginMutation.isError
    }
  }

  const header = (
    <div className="p-6 pb-0">
      <h2 className="text-2xl font-semibold tracking-tight text-slate-900">Log in</h2>
      <p className="mt-1 text-sm text-slate-500">Welcome back.</p>
    </div>
  );

  return (
    <div className="flex min-h-[70vh] items-center justify-center">
      <Card className="w-full max-w-md" header={header}>
        <form className="grid gap-6" onSubmit={handleSubmit}>
          {loginMutation.isError && (
            <Message
              className="w-full"
              severity="error"
              text="Invalid credentials. Please try again."
            />
          )}

          <FloatLabel>
            <InputText
              autoComplete="email"
              className="w-full"
              id="email"
              required
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            <label htmlFor="email">Email</label>
          </FloatLabel>

          <FloatLabel>
            <Password
              className="w-full"
              feedback={false}
              inputId="password"
              inputStyle={{ width: "100%" }}
              required
              toggleMask
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <label htmlFor="password">Password</label>
          </FloatLabel>

          <Button
            className="w-full"
            disabled={loginMutation.isPending}
            label={loginMutation.isPending ? "Logging in…" : "Log in"}
            type="submit"
          />

          <p className="text-center text-sm text-slate-600">
            No account yet?{" "}
            <button
              className="font-medium text-primary-600 underline-offset-4 hover:underline"
              type="button"
              onClick={() => navigate("/register")}
            >
              Create one
            </button>
          </p>
        </form>
      </Card>
    </div>
  );
}
