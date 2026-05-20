param(
    [string]$JobName = "HealthCheck"
)

$ErrorActionPreference = "Stop"

function Resolve-DotnetPath {
    $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -ne $dotnetCmd) {
        return $dotnetCmd.Source
    }

    $fallback = "/home/vscode/.dotnet/dotnet"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "dotnet executable not found."
}


$repoRootCandidates = @(
    (Split-Path -Parent $PSScriptRoot),
    (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
)

$repoRoot = $repoRootCandidates |
    Where-Object { Test-Path (Join-Path $_ "Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj") } |
    Select-Object -First 1

$workerProject = if (-not [string]::IsNullOrWhiteSpace($repoRoot)) {
    Join-Path $repoRoot "Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj"
} else {
    $null
}

if (-not (Test-Path $workerProject)) {
    throw "Worker project not found at $workerProject"
}

$dotnetPath = Resolve-DotnetPath

if (-not $env:ASPNETCORE_ENVIRONMENT) {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
}

if (-not $env:DOTNET_ENVIRONMENT) {
    $env:DOTNET_ENVIRONMENT = $env:ASPNETCORE_ENVIRONMENT
}

if (-not $env:BATCH_RUN_MODE) {
    $env:BATCH_RUN_MODE = "Manual"
}

if (-not $env:BATCH_JOBQUEUE_ID) {
    $env:BATCH_JOBQUEUE_ID = [guid]::NewGuid().ToString()
}

if (-not $env:BATCH_USER_ID) {
    $env:BATCH_USER_ID = "system"
}

if (-not $env:ConnectionStrings__BatchJobsConnectionString) {
    $localSettingsPath = Join-Path $repoRoot "appsettings.Local.json"
    if (-not (Test-Path $localSettingsPath)) {
        throw "Connection string not set. Provide ConnectionStrings__BatchJobsConnectionString or create appsettings.Local.json at $localSettingsPath"
    }
}

Push-Location $repoRoot
try {
    & $dotnetPath run --project $workerProject -- $JobName
    if ($LASTEXITCODE -ne 0) {
        throw "Worker execution failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
