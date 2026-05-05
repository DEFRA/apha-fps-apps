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

$repoRoot = Split-Path -Parent $PSScriptRoot
$workerProject = Join-Path $repoRoot "Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj"

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
