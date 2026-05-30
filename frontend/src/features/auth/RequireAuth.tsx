import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/useAuth";

/**
 * RequireAuth — only passes fully authenticated users through.
 * Redirects guest and unauthenticated visitors to /welcome.
 */
export function RequireAuth() {
  const { mode } = useAuth();

  if (mode !== "authenticated") {
    return <Navigate to="/welcome" replace />;
  }

  return <Outlet />;
}
