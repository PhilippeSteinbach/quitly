import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/useAuth";

/**
 * RequireSession — passes guest and authenticated users through.
 * Redirects unauthenticated visitors to /welcome.
 */
export function RequireSession() {
  const { mode } = useAuth();

  if (mode === "unauthenticated") {
    return <Navigate to="/welcome" replace />;
  }

  return <Outlet />;
}
