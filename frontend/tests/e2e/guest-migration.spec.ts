import { test, expect } from "@playwright/test";

/**
 * T048: Guest-to-Registered migration E2E tests
 * Covers: guest with data → Register page shows export banner → download → registration clears guest keys.
 */

test.describe("Guest migration prompt", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.evaluate(() => {
      localStorage.clear();
      // Set up guest mode with data
      localStorage.setItem("quitly.auth-mode", "guest");
      localStorage.setItem(
        "quitly.guest.habit",
        JSON.stringify({
          title: "Quit smoking",
          category: "smoking",
          timezone: "UTC",
          createdAt: "2025-01-01T00:00:00.000Z"
        })
      );
      localStorage.setItem(
        "quitly.guest.checkins",
        JSON.stringify([
          { date: "2025-01-10", status: "abstinent", mood: "neutral", triggers: [], note: "" }
        ])
      );
    });
  });

  test("Register page shows export banner when guest has data", async ({ page }) => {
    await page.goto("/register");
    await expect(page.getByText(/local guest data/i)).toBeVisible();
    await expect(page.getByRole("button", { name: /download json backup/i })).toBeVisible();
  });

  test("Download backup button triggers file download", async ({ page }) => {
    await page.goto("/register");

    const downloadPromise = page.waitForEvent("download");
    await page.getByRole("button", { name: /download json backup/i }).click();

    const download = await downloadPromise;
    expect(download.suggestedFilename()).toMatch(/quitly-backup-\d{4}-\d{2}-\d{2}\.json/);
  });

  test("After registration, guest data is cleared", async ({ page }) => {
    // Mock the register API call
    await page.route("**/api/v1/auth/register", (route) =>
      route.fulfill({
        status: 201,
        contentType: "application/json",
        body: JSON.stringify({
          accessToken: "access-token",
          refreshToken: "refresh-token"
        })
      })
    );
    await page.route("**/api/v1/auth/me", (route) =>
      route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ id: "new-user-id", email: "new@example.com" })
      })
    );

    await page.goto("/register");
    await page.getByLabel(/email/i).fill("new@example.com");
    await page.getByLabel(/password/i).fill("Password123!");
    await page.getByRole("button", { name: /create account/i }).click();

    // Confirm dialog should appear
    await expect(page.getByText(/cannot be undone/i)).toBeVisible();
    await page.getByRole("button", { name: /continue/i }).click();

    // Guest keys should be cleared
    const habit = await page.evaluate(() => localStorage.getItem("quitly.guest.habit"));
    expect(habit).toBeNull();

    const checkIns = await page.evaluate(() => localStorage.getItem("quitly.guest.checkins"));
    expect(checkIns).toBeNull();
  });

  test("No export banner shown when guest has no data", async ({ page }) => {
    await page.evaluate(() => {
      localStorage.removeItem("quitly.guest.habit");
    });

    await page.goto("/register");
    await expect(page.getByText(/local guest data/i)).not.toBeVisible();
  });
});
