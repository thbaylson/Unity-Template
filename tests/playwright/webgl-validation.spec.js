const { test, expect } = require("@playwright/test");

const titleNewGameTarget = { x: 0.5, y: 0.585 };
const titleSettingsTarget = { x: 0.5, y: 0.775 };
const settingsControlsTabTarget = { x: 0.542, y: 0.352 };
const pauseResumeTarget = { x: 0.085, y: 0.39 };
const pauseSettingsTarget = { x: 0.085, y: 0.64 };

const knownBenignConsolePatterns = [
  /FS\.syncfs operations in flight at once/i,
  /WebGL save sync .*failed/i,
  /PlayerPrefs will not be saved/i
];

function collectBrowserErrors(page) {
  const browserErrors = [];
  page.on("pageerror", error => browserErrors.push(String(error)));
  page.on("console", message => {
    const text = message.text();
    if (message.type() === "error" && !knownBenignConsolePatterns.some(pattern => pattern.test(text))) {
      browserErrors.push(text);
    }
  });

  return browserErrors;
}

async function waitForUnityLoadingToFinish(page) {
  await page.waitForFunction(() => {
    const canvas = document.querySelector("#unity-canvas");
    const loadingBar = document.querySelector("#unity-loading-bar");
    return !!canvas && (!loadingBar || getComputedStyle(loadingBar).display === "none");
  }, undefined, { timeout: 180000 });
}

async function loadUnity(page) {
  await page.goto("/");

  const canvas = page.locator("#unity-canvas");
  await expect(canvas).toBeVisible();
  await waitForUnityLoadingToFinish(page);
  await page.waitForTimeout(2500);
  return canvas;
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
  return canvas.screenshot({ path: testInfo.outputPath(fileName) });
}

async function pressNavigationKey(page, key, delayMs = 100) {
  await page.keyboard.press(key);
  await page.waitForTimeout(delayMs);
}

function bufferDifferenceRatio(first, second) {
  const comparedLength = Math.min(first.length, second.length);
  if (comparedLength === 0) {
    return 0;
  }

  let changedBytes = Math.abs(first.length - second.length);
  for (let index = 0; index < comparedLength; index += 1) {
    if (first[index] !== second[index]) {
      changedBytes += 1;
    }
  }

  return changedBytes / Math.max(first.length, second.length);
}

async function startFlatScene(page, canvas) {
  await clickCanvasTarget(page, canvas, titleNewGameTarget);
  await page.waitForTimeout(2500);
}

test("settings tabs render as a cohesive audio-to-controls flow", async ({ page }, testInfo) => {
  const browserErrors = collectBrowserErrors(page);
  const canvas = await loadUnity(page);

  await captureCanvas(canvas, testInfo, "webgl-loaded.png");

  await clickCanvasTarget(page, canvas, titleSettingsTarget);
  await page.waitForTimeout(750);
  const audioTab = await captureCanvas(canvas, testInfo, "settings-audio-tab.png");

  await clickCanvasTarget(page, canvas, settingsControlsTabTarget);
  await page.waitForTimeout(750);
  const controlsTab = await captureCanvas(canvas, testInfo, "settings-controls-tab.png");

  await page.keyboard.press("ArrowRight");
  await page.waitForTimeout(250);
  const controlsNavigation = await captureCanvas(canvas, testInfo, "settings-controls-navigation.png");

  for (let index = 0; index < 12; index += 1) {
    await pressNavigationKey(page, "ArrowDown", 50);
  }

  await pressNavigationKey(page, "Enter", 50);
  await page.waitForTimeout(750);
  const closedFromControls = await captureCanvas(canvas, testInfo, "settings-controls-closed-from-save-close.png");

  expect(bufferDifferenceRatio(audioTab, controlsTab)).toBeGreaterThan(0.05);
  expect(bufferDifferenceRatio(controlsTab, controlsNavigation)).toBeGreaterThan(0.001);
  expect(bufferDifferenceRatio(controlsTab, closedFromControls)).toBeGreaterThan(0.05);
  expect(browserErrors).toEqual([]);
});

test("pause menu keeps deterministic vertical navigation order", async ({ page }, testInfo) => {
  const browserErrors = collectBrowserErrors(page);
  const canvas = await loadUnity(page);
  await startFlatScene(page, canvas);

  await page.keyboard.press("Escape");
  await page.waitForTimeout(500);

  await pressNavigationKey(page, "ArrowDown");
  await pressNavigationKey(page, "ArrowDown");
  await page.waitForTimeout(250);
  const achievementsSelected = await captureCanvas(canvas, testInfo, "pause-achievements-selected.png");

  await pressNavigationKey(page, "ArrowDown");
  await page.waitForTimeout(250);
  const settingsSelected = await captureCanvas(canvas, testInfo, "pause-settings-selected.png");

  await pressNavigationKey(page, "ArrowUp");
  await page.waitForTimeout(250);
  const upFromSettings = await captureCanvas(canvas, testInfo, "pause-up-from-settings.png");

  await pressNavigationKey(page, "ArrowDown");
  await pressNavigationKey(page, "ArrowDown");
  await pressNavigationKey(page, "ArrowUp");
  await page.waitForTimeout(250);
  const upFromReturnToTitle = await captureCanvas(canvas, testInfo, "pause-up-from-return-to-title.png");

  expect(bufferDifferenceRatio(achievementsSelected, upFromSettings)).toBeLessThan(0.01);
  expect(bufferDifferenceRatio(settingsSelected, upFromReturnToTitle)).toBeLessThan(0.01);
  expect(browserErrors).toEqual([]);
});

test("title screen remains navigable after returning from an opened pause menu", async ({ page }, testInfo) => {
  const browserErrors = collectBrowserErrors(page);
  const canvas = await loadUnity(page);
  await startFlatScene(page, canvas);

  await page.keyboard.press("Escape");
  await page.waitForTimeout(500);

  for (let index = 0; index < 4; index += 1) {
    await pressNavigationKey(page, "ArrowDown");
  }

  await pressNavigationKey(page, "Enter");
  await page.waitForTimeout(2000);
  const returnedTitle = await captureCanvas(canvas, testInfo, "title-after-pause-return.png");

  await pressNavigationKey(page, "ArrowDown");
  await pressNavigationKey(page, "Enter");
  await page.waitForTimeout(750);
  const settingsFromReturnedTitle = await captureCanvas(canvas, testInfo, "settings-from-returned-title-navigation.png");

  expect(bufferDifferenceRatio(returnedTitle, settingsFromReturnedTitle)).toBeGreaterThan(0.05);
  expect(browserErrors).toEqual([]);
});

test("pause menu opened by Escape can be resumed with the mouse", async ({ page }, testInfo) => {
  const browserErrors = collectBrowserErrors(page);
  const canvas = await loadUnity(page);
  await startFlatScene(page, canvas);

  await page.keyboard.press("Escape");
  await page.waitForTimeout(500);
  const paused = await captureCanvas(canvas, testInfo, "pause-menu-from-escape.png");

  await clickCanvasTarget(page, canvas, pauseResumeTarget);
  await page.waitForTimeout(750);
  const resumed = await captureCanvas(canvas, testInfo, "pause-menu-after-mouse-resume.png");

  expect(bufferDifferenceRatio(paused, resumed)).toBeGreaterThan(0.05);
  expect(browserErrors).toEqual([]);
});

test("settings screen keeps ownership when pause input is pressed again", async ({ page }, testInfo) => {
  const browserErrors = collectBrowserErrors(page);
  const canvas = await loadUnity(page);
  await startFlatScene(page, canvas);

  await page.keyboard.press("Escape");
  await page.waitForTimeout(500);
  await clickCanvasTarget(page, canvas, pauseSettingsTarget);
  await page.waitForTimeout(750);
  const settingsBeforePauseKey = await captureCanvas(canvas, testInfo, "pause-settings-before-pause-key.png");

  await page.keyboard.press("Escape");
  await page.waitForTimeout(750);
  const settingsAfterPauseKey = await captureCanvas(canvas, testInfo, "pause-settings-after-pause-key.png");

  expect(bufferDifferenceRatio(settingsBeforePauseKey, settingsAfterPauseKey)).toBeLessThan(0.02);
  expect(browserErrors).toEqual([]);
});
