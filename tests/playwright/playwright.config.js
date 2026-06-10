const path = require("node:path");
const { defineConfig } = require("@playwright/test");

const serverScriptPath = path.resolve(__dirname, "serve-unity-webgl.mjs");
const webGlBuildPath = process.env.UNITY_WEBGL_BUILD_PATH
  ? path.resolve(process.env.UNITY_WEBGL_BUILD_PATH)
  : path.resolve(__dirname, "../../Build/WebGL");
const port = Number.parseInt(process.env.UNITY_WEBGL_PORT ?? "4173", 10);

module.exports = defineConfig({
  testDir: __dirname,
  testMatch: "webgl-smoke.spec.js",
  timeout: 180000,
  expect: {
    timeout: 60000
  },
  fullyParallel: false,
  workers: 1,
  reporter: [
    ["list"],
    ["html", {
      open: "never",
      outputFolder: path.resolve(__dirname, "playwright-report")
    }]
  ],
  outputDir: path.resolve(__dirname, "test-results"),
  use: {
    browserName: "chromium",
    baseURL: `http://127.0.0.1:${port}`,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
    viewport: {
      width: 1280,
      height: 720
    }
  },
  projects: [
    {
      name: "chromium"
    }
  ],
  webServer: {
    command: `node "${serverScriptPath}" --root "${webGlBuildPath}" --port ${port}`,
    url: `http://127.0.0.1:${port}`,
    reuseExistingServer: !process.env.CI,
    timeout: 30000
  }
});
