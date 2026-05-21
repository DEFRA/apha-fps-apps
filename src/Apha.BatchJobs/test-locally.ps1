<#
.SYNOPSIS
One-command local testing script for Batch Jobs foundation layer.
#>

param(
    [switch]$Clean,
    [switch]$LogsOnly,
    [switch]$Stop,
    [switch]$NoPrompt,
    [switch]$Native,
    [ValidateSet("withdb", "nodb")]
    [string]$DockerProfile = "withdb",
    [string]$JobName = "HealthCheck",
    [string]$JobQueueId,
    [string]$UserId = "local-dev"
)

$ErrorActionPreference = "Stop"
$BatchJobsDir = $PSScriptRoot
$script:ExecutionMode = $null

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
}

function Resolve-JobQueueId {
    param([string]$InputJobQueueId)

    if (-not [string]::IsNullOrWhiteSpace($InputJobQueueId)) {
        $parsed = [Guid]::Empty
        if ([Guid]::TryParse($InputJobQueueId, [ref]$parsed)) {
            return $parsed.ToString()
        }

        throw "JobQueueId must be a valid UUID. Received: $InputJobQueueId"
    }

    return [Guid]::NewGuid().ToString()
}

function Set-WorkerRuntimeEnvironment {
    param(
        [string]$ResolvedJobName,
        [string]$ResolvedJobQueueId,
        [string]$ResolvedUserId
    )

    $env:BATCH_JOB_NAME = $ResolvedJobName
    $env:BATCH_RUN_MODE = "AdHoc"
    $env:BATCH_JOBQUEUE_ID = $ResolvedJobQueueId
    $env:BATCH_USER_ID = $ResolvedUserId

    Write-Host "Simulated trigger payload:" -ForegroundColor Yellow
    Write-Host "  BATCH_JOB_NAME=$ResolvedJobName"
    Write-Host "  BATCH_RUN_MODE=AdHoc"
    Write-Host "  BATCH_JOBQUEUE_ID=$ResolvedJobQueueId"
    Write-Host "  BATCH_USER_ID=$ResolvedUserId"
}

function Test-Docker {
    Write-Host "Checking Docker..." -ForegroundColor Yellow
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Failure "Docker not found in PATH."
        return $false
    }

    try {
        docker ps *> $null
        Write-Success "Docker is available"
        return $true
    } catch {
        Write-Failure "Docker daemon not running."
        return $false
    }
}

function Get-ExecutionMode {
    if ($Native) {
        return "native"
    }

    $dockerAvailable = Test-Docker
    if (-not $dockerAvailable) {
        return "native"
    }

    try {
        $dockerOsType = (docker info --format "{{.OSType}}" 2>$null).Trim()
        if ($dockerOsType -eq "linux") {
            return "docker"
        }

        if ($dockerOsType -eq "windows") {
            Write-Host "Docker daemon is running in Windows container mode; this stack targets Linux images, so native .NET mode will be used instead." -ForegroundColor Yellow
            return "native"
        }
    } catch {
        Write-Host "Unable to determine Docker container mode, falling back to native .NET mode." -ForegroundColor Yellow
    }

    return "native"
}

function Stop-Containers {
    Write-Header "Stopping Containers"
    Push-Location $BatchJobsDir
    docker-compose --profile $DockerProfile down --remove-orphans 2>$null
    Pop-Location
    Write-Success "Containers stopped"
}

function Clean-Environment {
    Write-Header "Cleaning Environment"
    Push-Location $BatchJobsDir
    docker-compose --profile $DockerProfile down -v --remove-orphans 2>$null
    docker-compose --profile $DockerProfile rm -f 2>$null
    Pop-Location
    Write-Success "Environment cleaned"
}

function Build-And-Run {
    Write-Header "Building Docker Image"
    try {
        Push-Location $BatchJobsDir
        docker-compose --profile $DockerProfile build
        Write-Success "Image built successfully"

        Write-Header "Starting Services with docker-compose"
        $serviceName = if ($DockerProfile -eq "nodb") { "batch-jobs-nodb" } else { "batch-jobs-withdb" }
        if ($DockerProfile -eq "nodb") {
            Write-Host "Starting Batch Job container in NoDb mode..." -ForegroundColor Yellow
        } else {
            Write-Host "Starting PostgreSQL and Batch Job..." -ForegroundColor Yellow
        }
        Write-Host "Streaming logs until the batch job exits..." -ForegroundColor Yellow
        docker-compose --profile $DockerProfile up --no-build --remove-orphans --abort-on-container-exit --exit-code-from $serviceName
        Pop-Location
    } catch {
        Pop-Location
        Write-Failure "docker-compose failed: $_"
        exit 1
    }
}

function Run-Native {
    Write-Header "Running Native .NET Validation"
    Push-Location $BatchJobsDir
    try {
        dotnet build
        dotnet test .\Apha.BatchJobs.UnitTests\Apha.BatchJobs.UnitTests.csproj --no-build
        dotnet run --project .\Apha.BatchJobs.Worker\Apha.BatchJobs.Worker.csproj -- $JobName
    } finally {
        Pop-Location
    }
}

function Show-Logs {
    Write-Header "Showing Logs"
    $serviceName = if ($DockerProfile -eq "nodb") { "batch-jobs-nodb" } else { "batch-jobs-withdb" }
    Push-Location $BatchJobsDir
    docker-compose --profile $DockerProfile logs -f $serviceName
    Pop-Location
}

function Show-Status {
    Write-Header "Container Status"
    Push-Location $BatchJobsDir
    docker-compose --profile $DockerProfile ps
    Pop-Location
}

Write-Host "Batch Jobs - Local Testing Script" -ForegroundColor Cyan
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')]" -ForegroundColor Gray

$resolvedJobQueueId = Resolve-JobQueueId -InputJobQueueId $JobQueueId
Set-WorkerRuntimeEnvironment -ResolvedJobName $JobName -ResolvedJobQueueId $resolvedJobQueueId -ResolvedUserId $UserId

$script:ExecutionMode = Get-ExecutionMode
Write-Host "Execution mode: $script:ExecutionMode" -ForegroundColor Yellow
Write-Host "Docker profile: $DockerProfile" -ForegroundColor Yellow

if ($Stop) {
    if ($script:ExecutionMode -eq "docker") {
        Stop-Containers
    } else {
        Write-Host "Stop is only applicable in docker mode." -ForegroundColor Yellow
    }
    exit 0
}

if ($LogsOnly) {
    if ($script:ExecutionMode -eq "docker") {
        Show-Logs
    } else {
        Write-Host "LogsOnly is only applicable in docker mode." -ForegroundColor Yellow
    }
    exit 0
}

if ($Clean -and $script:ExecutionMode -eq "docker") {
    Clean-Environment
}

Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
if ($script:ExecutionMode -eq "docker") {
    if ($DockerProfile -eq "nodb") {
        Write-Host "  1. Build Docker image"
        Write-Host "  2. Start BatchJobs container without PostgreSQL"
        Write-Host "  3. Run $JobName job in NoDb mode"
    } else {
        Write-Host "  1. Build Docker image"
        Write-Host "  2. Start PostgreSQL container"
        Write-Host "  3. Run $JobName job in WithDb mode"
    }
    Write-Host "  4. Stream logs"
} else {
    Write-Host "  1. Build the .NET project"
    Write-Host "  2. Run the local unit test suite"
    Write-Host "  3. Run $JobName natively"
    Write-Host "  4. Validate worker bootstrap, logging, and job execution"
}
Write-Host ""

if (-not $NoPrompt) {
    Read-Host "Press Enter to continue"
}

if ($script:ExecutionMode -eq "docker") {
    Build-And-Run
    Show-Status
} else {
    Run-Native
}
