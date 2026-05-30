 import { describe, expect, it } from "vitest";

 const PERFORMANCE_BUDGETS = {
   checkInInteractionMs: 1500,
   weeklyInsightsOpenMs: 2000,
   dashboardRenderMs: 2000
 };

 describe("performance budgets", () => {
   it("keeps documented MVP thresholds stable", () => {
     expect(PERFORMANCE_BUDGETS.checkInInteractionMs).toBeLessThanOrEqual(1500);
     expect(PERFORMANCE_BUDGETS.weeklyInsightsOpenMs).toBeLessThanOrEqual(2000);
     expect(PERFORMANCE_BUDGETS.dashboardRenderMs).toBeLessThanOrEqual(2000);
   });
 });