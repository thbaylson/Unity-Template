[CmdletBinding()]
param(
    [string]$UnityPath,
    [string]$ProjectPath,
    [string]$ResultsPath,
    [string]$LogPath,
    [string]$TestFilter,
    [string]$AssemblyNames
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ProjectRoot {
    if ($ProjectPath) {
        return (Resolve-Path $ProjectPath).Path
    }

    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

function Get-EditorVersion {
    param(
        [string]$ProjectRoot
    )

    $versionFile = Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'
    $match = Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(.+)$'
    if (-not $match) {
        throw "Could not determine Unity editor version from '$versionFile'."
    }

    return $match.Matches[0].Groups[1].Value.Trim()
}

function Resolve-UnityPath {
    param(
        [string]$ConfiguredUnityPath,
        [string]$ProjectRoot
    )

    if ($ConfiguredUnityPath) {
        return (Resolve-Path $ConfiguredUnityPath).Path
    }

    $editorVersion = Get-EditorVersion -ProjectRoot $ProjectRoot
    $defaultUnityPath = Join-Path "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor" 'Unity.exe'

    if (-not (Test-Path $defaultUnityPath)) {
        throw "Unity executable was not found at '$defaultUnityPath'. Pass -UnityPath to override it."
    }

    return $defaultUnityPath
}

function Get-ProjectSettingValue {
    param(
        [string]$ProjectRoot,
        [string]$SettingName,
        [string]$FallbackValue
    )

    $settingsPath = Join-Path $ProjectRoot 'ProjectSettings\ProjectSettings.asset'
    $match = Select-String -Path $settingsPath -Pattern "^\s*${SettingName}:\s*(.+)$"
    if ($match) {
        return $match.Matches[0].Groups[1].Value.Trim()
    }

    return $FallbackValue
}

function Get-LatestResultPathFromEditorLog {
    param(
        [string]$EditorLogPath
    )

    if (-not (Test-Path $EditorLogPath)) {
        return $null
    }

    $match = Select-String -Path $EditorLogPath -Pattern 'Saving results to:\s*(.+)$' | Select-Object -Last 1
    if (-not $match) {
        return $null
    }

    return $match.Matches[0].Groups[1].Value.Trim()
}

$projectRoot = Get-ProjectRoot
$unityExecutable = Resolve-UnityPath -ConfiguredUnityPath $UnityPath -ProjectRoot $projectRoot
$logsRoot = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Force $logsRoot | Out-Null

if (-not $ResultsPath) {
    $ResultsPath = Join-Path $logsRoot 'EditModeTestResults.xml'
}

if (-not $LogPath) {
    $LogPath = Join-Path $logsRoot 'UnityEditModeTests.log'
}

$resultsPath = [System.IO.Path]::GetFullPath($ResultsPath)
$logPath = [System.IO.Path]::GetFullPath($LogPath)
$editorLogPath = Join-Path $env:LOCALAPPDATA 'Unity\Editor\Editor.log'

if (Test-Path $resultsPath) {
    Remove-Item $resultsPath -Force
}

if (Test-Path $logPath) {
    Remove-Item $logPath -Force
}

$companyName = Get-ProjectSettingValue -ProjectRoot $projectRoot -SettingName 'companyName' -FallbackValue 'DefaultCompany'
$productName = Get-ProjectSettingValue -ProjectRoot $projectRoot -SettingName 'productName' -FallbackValue (Split-Path $projectRoot -Leaf)
$fallbackResultsPath = Join-Path $env:USERPROFILE "AppData\LocalLow\$companyName\$productName\TestResults.xml"

$unityArguments = @(
    '-batchmode',
    '-projectPath', $projectRoot,
    '-runTests',
    '-testPlatform', 'editmode',
    '-testResults', $resultsPath,
    '-logFile', $logPath,
    '-quit'
)

if ($TestFilter) {
    $unityArguments += @('-testFilter', $TestFilter)
}

if ($AssemblyNames) {
    $unityArguments += @('-assemblyNames', $AssemblyNames)
}

Write-Host "Running Unity EditMode tests..."
Write-Host "Unity:   $unityExecutable"
Write-Host "Project: $projectRoot"

$global:LASTEXITCODE = 0
& $unityExecutable @unityArguments
$unityExitCode = $LASTEXITCODE
$resolvedResultsPath = $null
if (Test-Path $resultsPath) {
    $resolvedResultsPath = $resultsPath
}
else {
    $editorLogResultPath = Get-LatestResultPathFromEditorLog -EditorLogPath $editorLogPath
    if ($editorLogResultPath -and (Test-Path $editorLogResultPath)) {
        Copy-Item $editorLogResultPath $resultsPath -Force
        $resolvedResultsPath = $resultsPath
    }
    elseif (Test-Path $fallbackResultsPath) {
        Copy-Item $fallbackResultsPath $resultsPath -Force
        $resolvedResultsPath = $resultsPath
    }
}

if (-not $resolvedResultsPath) {
    Write-Error "Unity exited with code $unityExitCode and no XML test results were found. Check '$editorLogPath'."
    exit 1
}

[xml]$resultsXml = Get-Content $resolvedResultsPath
$testRun = $resultsXml.'test-run'
if (-not $testRun) {
    Write-Error "Test results file '$resolvedResultsPath' did not contain a <test-run> element."
    exit 1
}

$total = [int]$testRun.total
$passed = [int]$testRun.passed
$failed = [int]$testRun.failed
$skipped = [int]$testRun.skipped
$result = [string]$testRun.result

Write-Host "Results: $result"
Write-Host "Total:   $total"
Write-Host "Passed:  $passed"
Write-Host "Failed:  $failed"
Write-Host "Skipped: $skipped"
Write-Host "XML:     $resolvedResultsPath"

if ($unityExitCode -ne 0) {
    Write-Warning "Unity exited with code $unityExitCode. Using XML test results as the source of truth."
}

if ($result -ieq 'Passed' -and $failed -eq 0) {
    exit 0
}

exit 1

