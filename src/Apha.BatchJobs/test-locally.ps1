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
    [string]$JobName = "HealthCheck"
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
    docker-compose down 2>$null
    Pop-Location
    Write-Success "Containers stopped"
}

function Clean-Environment {
    Write-Header "Cleaning Environment"
    Push-Location $BatchJobsDir
    docker-compose down -v 2>$null
    docker-compose rm -f 2>$null
    Pop-Location
    Write-Success "Environment cleaned"
}

function Build-And-Run {
    Write-Header "Building Docker Image"
    try {
        Push-Location $BatchJobsDir
        docker-compose build
        Write-Success "Image built successfully"

        Write-Header "Starting Services with docker-compose"
        Write-Host "Press Ctrl+C to stop viewing logs (containers continue running)" -ForegroundColor Gray
        docker-compose up --no-build
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
        dotnet run --project BatchJobs.csproj -- $JobName
    } finally {
        Pop-Location
    }
}

function Show-Logs {
    Write-Header "Showing Logs"
    Push-Location $BatchJobsDir
    docker-compose logs -f batch-jobs
    Pop-Location
}

function Show-Status {
    Write-Header "Container Status"
    Push-Location $BatchJobsDir
    docker-compose ps
    Pop-Location
}

Write-Host "Batch Jobs - Local Testing Script" -ForegroundColor Cyan
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')]" -ForegroundColor Gray

$script:ExecutionMode = Get-ExecutionMode
Write-Host "Execution mode: $script:ExecutionMode" -ForegroundColor Yellow

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
    Write-Host "  1. Build Docker image"
    Write-Host "  2. Start PostgreSQL container"
    Write-Host "  3. Run $JobName job"
    Write-Host "  4. Stream logs"
} else {
    Write-Host "  1. Build the .NET project"
    Write-Host "  2. Run $JobName natively"
    Write-Host "  3. Validate worker bootstrap, logging, and job execution"
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
