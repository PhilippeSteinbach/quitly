import { createBrowserRouter } from "react-router-dom";
import { App } from "@/app/App";
import { CheckInPage } from "@/features/checkin/CheckInPage";
import { WeeklyInsightsPage } from "@/features/insights/WeeklyInsightsPage";
import { OnboardingPage } from "@/features/onboarding/OnboardingPage";
import { RecoveryFlowPage } from "@/features/recovery/RecoveryFlowPage";

function PlaceholderPage({
  title,
  description
}: {
  title: string;
  description: string;
}) {
  return (
    <section className="grid gap-4 rounded-[28px] border border-border bg-card p-8 shadow-soft">
      <span className="inline-flex w-fit rounded-full bg-secondary px-3 py-1 text-sm text-slate-700">
        MVP scaffold
      </span>
      <h2 className="text-2xl font-semibold tracking-tight">{title}</h2>
      <p className="max-w-2xl text-base leading-7 text-slate-600">{description}</p>
    </section>
  );
}

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        index: true,
        element: <OnboardingPage />
      },
      {
        path: "check-in",
        element: <CheckInPage />
      },
      {
        path: "recovery",
        element: <RecoveryFlowPage />
      },
      {
        path: "insights",
        element: <WeeklyInsightsPage />
      }
    ]
  }
]);


export { PlaceholderPage };

