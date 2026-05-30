import { createBrowserRouter, Navigate } from "react-router-dom";
import { App } from "@/app/App";
import { CheckInPage } from "@/features/checkin/CheckInPage";
import { WeeklyInsightsPage } from "@/features/insights/WeeklyInsightsPage";
import { OnboardingPage } from "@/features/onboarding/OnboardingPage";
import { PostMvpPlaceholder, isPostMvpEnabled } from "@/features/postmvp/PostMvpPlaceholder";
import { RecoveryFlowPage } from "@/features/recovery/RecoveryFlowPage";
import { RequireAuth } from "@/features/auth/RequireAuth";
import { RequireSession } from "@/features/auth/RequireSession";
import { WelcomePage } from "@/features/auth/WelcomePage";
import { LoginPage } from "@/features/auth/LoginPage";
import { RegisterPage } from "@/features/auth/RegisterPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      // Default redirect: unauthenticated → /welcome (handled by RequireSession inside protected routes)
      {
        index: true,
        element: <Navigate to="/onboarding" replace />
      },

      // Public routes (no session required)
      {
        path: "welcome",
        element: <WelcomePage />
      },
      {
        path: "login",
        element: <LoginPage />
      },
      {
        path: "register",
        element: <RegisterPage />
      },

      // Protected routes — require at least guest or authenticated session
      {
        element: <RequireSession />,
        children: [
          {
            path: "onboarding",
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
      },

      // T022b: Protected routes — require full authentication
      {
        element: <RequireAuth />,
        children: [
          {
            path: "account",
            element: (
              <PostMvpPlaceholder
                feature="Account"
                description="Account settings and profile management will be available here."
              />
            )
          }
        ]
      },

      ...(isPostMvpEnabled()
        ? [
            {
              path: "achievements",
              element: (
                <PostMvpPlaceholder
                  feature="Achievements"
                  description="Achievement badges remain a Post-MVP exploration; this route is only mounted when the feature flag is on."
                />
              )
            },
            {
              path: "interventions",
              element: (
                <PostMvpPlaceholder
                  feature="Interventions"
                  description="Adaptive interventions remain a Post-MVP exploration; the route is only mounted when the feature flag is on."
                />
              )
            }
          ]
        : [])
    ]
  }
]);


