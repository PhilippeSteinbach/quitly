import { Tag } from "primereact/tag";
import { useAuth } from "@/features/auth/useAuth";

export function GuestModeBanner() {
  const { mode } = useAuth();

  if (mode !== "guest") return null;

  return <Tag severity="warning" value="Guest mode" />;
}
