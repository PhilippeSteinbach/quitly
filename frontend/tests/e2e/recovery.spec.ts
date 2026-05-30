import { test, expect } from "@playwright/test";

test("recovery screen renders", async ({ page }) => {
  await page.goto("/recovery");

  await expect(page.getByText(/a relapse does not erase progress/i)).toBeVisible();
  await expect(page.getByRole("button", { name: /record relapse/i })).toBeVisible();
});