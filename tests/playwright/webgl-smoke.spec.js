const { test, expect } = require("@playwright/test");

const titleSettingsFallbackTarget = { x: 0.5, y: 0.775 };
const knownBenignConsolePatterns = [
  /FS\.syncfs operations in flight at once/i,
  /WebGL save sync .*failed/i,
  /PlayerPrefs will not be saved/i
];

async function waitForUnityLoadingToFinish(page) {
  await page.waitForFunction(() => {
    const canvas = document.querySelector("#unity-canvas");
    const loadingBar = document.querySelector("#unity-loading-bar");
    return !!canvas && (!loadingBar || getComputedStyle(loadingBar).display === "none");
  }, undefined, { timeout: 180000 });
}

async function clickCanvasTarget(page, canvas, target) {
  const canvasBox = await canvas.boundingBox();
  expect(canvasBox).not.toBeNull();

  await page.mouse.click(
    canvasBox.x + (canvasBox.width * target.x),
    canvasBox.y + (canvasBox.height * target.y)
  );
}

async function captureCanvas(canvas, testInfo, fileName) {
  await canvas.screenshot({ path: testInfo.outputPath(fileName) });
}

test("loads WebGL and captures BetterInputHandling UI states", async ({ page }, testInfo) => {
  const browserErrors = [];
  page.on("pageerror", error => browserErrors.push(String(error)));
  page.on("console", message => {
    const text = message.text();
    if (message.type() === "error" && !knownBenignConsolePatterns.some(pattern => pattern.test(text))) {
      browserErrors.push(text);
    }
  });

  await page.goto("/");

  const canvas = page.locator("#unity-canvas");
  await expect(canvas).toBeVisible();
  await waitForUnityLoadingToFinish(page);
  await page.waitForTimeout(1000);
  await captureCanvas(canvas, testInfo, "webgl-loaded.png");

  await clickCanvasTarget(page, canvas, titleSettingsFallbackTarget);
  await page.waitForTimeout(750);
  await captureCanvas(canvas, testInfo, "settings-audio-tab.png");

  await page.keyboard.press("KeyE");
  await page.waitForTimeout(750);
  await captureCanvas(canvas, testInfo, "settings-controls-tab.png");

  expect(browserErrors).toEqual([]);
});
