import type { PropsWithChildren } from "react";

type ConsentGateProps = PropsWithChildren<{
  mvpMode?: boolean;
}>;

export function ConsentGate({ children, mvpMode = true }: ConsentGateProps) {
  if (!mvpMode) {
    return <>{children}</>;
  }

  return <>{children}</>;
}
