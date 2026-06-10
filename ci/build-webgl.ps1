param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe",
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$BuildPath = "Build\WebGL",
    [string]$LogPath = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..")).Path "Logs\UnityWebGLBuild.log")
)

if (-not (Test-Path $UnityPath)) {
    throw "Unity Editor was not found at '$UnityPath'."
}

$resolvedProjectPath = (Resolve-Path $ProjectPath).Path
$resolvedLogDirectory = Split-Path -Parent $LogPath
New-Item -ItemType Directory -Force -Path $resolvedLogDirectory | Out-Null

$unityArguments = @(
    "-batchmode",
    "-projectPath", "`"$resolvedProjectPath`"",
    "-executeMethod", "Template.Editor.CI.BuildWebGL",
    "-buildPath", "`"$BuildPath`"",
    "-quit",
    "-logFile", "`"$LogPath`""
)

$unityProcess = Start-Process `
    -FilePath $UnityPath `
    -ArgumentList $unityArguments `
    -Wait `
    -PassThru

if ($unityProcess.ExitCode -ne 0) {
    throw "Unity WebGL build failed. See '$LogPath'."
}
