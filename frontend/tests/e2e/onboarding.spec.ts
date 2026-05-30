import { test, expect } from "@playwright/test";

test("onboarding form renders", async ({ page }) => {
  await page.goto("/");

  await expect(page.getByText(/set one clear habit goal/i)).toBeVisible();
  await expect(page.getByRole("button", { name: /save active goal/i })).toBeVisible();
});