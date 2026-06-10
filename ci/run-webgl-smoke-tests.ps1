param(
    [switch]$SkipBuild,
    [switch]$SkipDependencyInstall,
    [switch]$SkipBrowserInstall,
    [switch]$Headed,
    [string]$BuildPath = ".\Build\WebGL",
    [int]$Port = 4173
)

$projectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$nodeModulesPath = Join-Path $projectPath "node_modules\@playwright\test"
$playwrightCachePath = Join-Path $env:LOCALAPPDATA "ms-playwright"

Push-Location $projectPath

try {
    if (-not $SkipBuild) {
        & (Join-Path $PSScriptRoot "build-webgl.ps1") -BuildPath $BuildPath
        if ($LASTEXITCODE -ne 0) {
            throw "Unity WebGL build failed."
        }
    }

    if (-not $SkipDependencyInstall -and -not (Test-Path $nodeModulesPath)) {
        & npm.cmd install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install failed."
        }
    }

    if (-not $SkipBrowserInstall -and -not (Test-Path $playwrightCachePath)) {
        & npx.cmd playwright install chromium
        if ($LASTEXITCODE -ne 0) {
            throw "Playwright browser install failed."
        }
    }

    $env:UNITY_WEBGL_BUILD_PATH = (Resolve-Path $BuildPath).Path
    $env:UNITY_WEBGL_PORT = $Port.ToString()

    $playwrightArgs = @("playwright", "test", "--config", "./tests/playwright/playwright.config.js")
    if ($Headed) {
        $playwrightArgs += "--headed"
    }

    & npx.cmd @playwrightArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Playwright smoke tests failed."
    }
}
finally {
    Remove-Item Env:\UNITY_WEBGL_BUILD_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:\UNITY_WEBGL_PORT -ErrorAction SilentlyContinue
    Pop-Location
}
