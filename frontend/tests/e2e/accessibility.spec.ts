import { test, expect } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";

/**
 * T046: Accessibility scans for auth and guest-mode pages.
 * Targets WCAG 2.1 AA — zero critical/serious violations required.
 */

const wcag21aa = { runOnly: { type: "tag", values: ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"] } };

async function runAxe(page: import("@playwright/test").Page) {
  return new AxeBuilder({ page })
    .options(wcag21aa)
    .analyze();
}

test.describe("Accessibility — Welcome page", () => {
  test("no critical/serious violations on /welcome", async ({ page }) => {
    await page.goto("/welcome");
    await expect(page.getByRole("button", { name: /continue as guest/i })).toBeVisible();

    const results = await runAxe(page);
    const serious = results.violations.filter((v) => v.impact === "critical" || v.impact === "serious");
    expect(serious, `Violations:\n${JSON.stringify(serious, null, 2)}`).toHaveLength(0);
  });
});

test.describe("Accessibility — Login page", () => {
  test("no critical/serious violations on /login", async ({ page }) => {
    await page.goto("/login");
    await expect(page.getByLabel(/email/i)).toBeVisible();

    const results = await runAxe(page);
    const serious = results.violations.filter((v) => v.impact === "critical" || v.impact === "serious");
    expect(serious, `Violations:\n${JSON.stringify(serious, null, 2)}`).toHaveLength(0);
  });
});

test.describe("Accessibility — Register page", () => {
  test("no critical/serious violations on /register", async ({ page }) => {
    await page.goto("/register");
    await expect(page.getByRole("button", { name: /create account/i })).toBeVisible();

    const results = await runAxe(page);
    const serious = results.violations.filter((v) => v.impact === "critical" || v.impact === "serious");
    expect(serious, `Violations:\n${JSON.stringify(serious, null, 2)}`).toHaveLength(0);
  });
});
