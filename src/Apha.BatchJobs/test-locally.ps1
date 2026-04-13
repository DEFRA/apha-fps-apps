<#
.SYNOPSIS
One-command local testing script for Batch Jobs foundation layer.
#>

param(
    [switch]$Clean,
    [switch]$LogsOnly,
    [switch]$Stop
)

$ErrorActionPreference = "Stop"

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
        Write-Failure "Docker not found in PATH. Please install Docker Desktop."
        exit 1
    }

    try {
        docker ps *> $null
        Write-Success "Docker is available"
    } catch {
        Write-Failure "Docker daemon not running. Please start Docker Desktop."
        exit 1
    }
}

function Stop-Containers {
    Write-Header "Stopping Containers"
    Push-Location src/Apha.BatchJobs
    docker-compose down 2>$null
    Pop-Location
    Write-Success "Containers stopped"
}

function Clean-Environment {
    Write-Header "Cleaning Environment"
    Push-Location src/Apha.BatchJobs
    docker-compose down -v 2>$null
    docker-compose rm -f 2>$null
    Pop-Location
    Write-Success "Environment cleaned"
}

function Build-And-Run {
    Write-Header "Building Docker Image"
    try {
        Push-Location src/Apha.BatchJobs
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

function Show-Logs {
    Write-Header "Showing Logs"
    Push-Location src/Apha.BatchJobs
    docker-compose logs -f batch-jobs
    Pop-Location
}

function Show-Status {
    Write-Header "Container Status"
    Push-Location src/Apha.BatchJobs
    docker-compose ps
    Pop-Location
}

Write-Host "Batch Jobs - Local Testing Script" -ForegroundColor Cyan
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')]" -ForegroundColor Gray

Test-Docker

if ($Stop) {
    Stop-Containers
    exit 0
}

if ($LogsOnly) {
    Show-Logs
    exit 0
}

if ($Clean) {
    Clean-Environment
}

Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Build Docker image"
Write-Host "  2. Start PostgreSQL container"
Write-Host "  3. Run HealthCheck job"
Write-Host "  4. Stream logs"
Write-Host ""
Read-Host "Press Enter to continue"

Build-And-Run
Show-Status
