#!/usr/bin/env pwsh
<#
.SYNOPSIS
    One-command local testing script for Batch Jobs foundation layer.

.DESCRIPTION
    Builds the Docker image, starts PostgreSQL, runs the HealthCheck job,
    and shows real-time logs. Container exits automatically when job completes.

.PARAMETER Clean
    Remove containers and volumes before running (fresh start).

.PARAMETER LogsOnly
    Show logs from running containers without restarting.

.PARAMETER Stop
    Stop all containers.

.EXAMPLE
    # First time setup (builds image, builds DB, runs job)
    ./test-locally.ps1

    # Show logs if containers are still running
    ./test-locally.ps1 -LogsOnly

    # Clean everything and start fresh
    ./test-locally.ps1 -Clean

    # Stop all containers
    ./test-locally.ps1 -Stop
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
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Test-Docker {
    Write-Host "Checking Docker..." -ForegroundColor Yellow
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Error "Docker not found in PATH. Please install Docker Desktop."
        exit 1
    }
    
    try {
        $output = docker ps 2>&1
        Write-Success "Docker is available"
    } catch {
        Write-Error "Docker daemon not running. Please start Docker Desktop."
        exit 1
    }
}

function Stop-Containers {
    Write-Header "Stopping Containers"
    docker-compose -f src/Apha.BatchJobs/docker-compose.yml down 2>$null
    Write-Success "Containers stopped"
}

function Clean-Environment {
    Write-Header "Cleaning Environment"
    Write-Host "Removing containers and volumes..." -ForegroundColor Yellow
    docker-compose -f src/Apha.BatchJobs/docker-compose.yml down -v 2>$null
    docker-compose -f src/Apha.BatchJobs/docker-compose.yml rm -f 2>$null
    Write-Success "Environment cleaned"
}

function Build-And-Run {
    Write-Header "Building Docker Image"
    try {
        Push-Location src/Apha.BatchJobs
        docker-compose build 2>&1 | ForEach-Object {
            if ($_ -match "error" -or $_ -match "failed") {
                Write-Host $_ -ForegroundColor Red
            } else {
                Write-Host $_
            }
        }
        Write-Success "Image built successfully"
        Pop-Location
    } catch {
        Write-Error "Docker build failed: $_"
        exit 1
    }

    Write-Header "Starting Services with docker-compose"
    try {
        Push-Location src/Apha.BatchJobs
        Write-Host ""
        Write-Host "Starting PostgreSQL and Batch Job..." -ForegroundColor Yellow
        Write-Host "Press Ctrl+C to stop (containers will continue running)" -ForegroundColor Gray
        Write-Host ""
        
        docker-compose up --no-build
        Pop-Location
    } catch {
        Write-Error "docker-compose up failed: $_"
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
    Write-Host ""
    Write-Header "Container Status Check"
    
    $containers = docker-compose -f src/Apha.BatchJobs/docker-compose.yml ps -q
    if ($containers) {
        docker-compose -f src/Apha.BatchJobs/docker-compose.yml ps
        
        # Check exit code of batch-jobs container
        $exitCode = docker wait batch-jobs-app 2>$null
        if ($exitCode) {
            Write-Host ""
            Write-Host "Batch job exit code: $exitCode" -ForegroundColor Yellow
            if ($exitCode -eq 0) {
                Write-Success "Job completed successfully"
            } else {
                Write-Error "Job failed with code: $exitCode"
            }
        }
    } else {
        Write-Host "No containers running"
    }
}

# Main script
Clear-Host
Write-Host "Batch Jobs - Local Testing Script" -ForegroundColor Cyan -NoNewline
Write-Host " [$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')]" -ForegroundColor Gray

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
Write-Host "  1. Build Docker image (BatchJobs)"
Write-Host "  2. Start PostgreSQL container"
Write-Host "  3. Run HealthCheck batch job"
Write-Host "  4. Stream logs to console"
Write-Host "  5. Exit when job completes"
Write-Host ""
Write-Host "You can Ctrl+C to stop viewing logs (containers keep running)"
Write-Host ""
Read-Host "Press Enter to continue"

Build-And-Run

Write-Host ""
Show-Status

Write-Host ""
Write-Host "TIP: View logs again with:  ./test-locally.ps1 -LogsOnly" -ForegroundColor Gray
Write-Host "TIP: Stop containers with: ./test-locally.ps1 -Stop" -ForegroundColor Gray
Write-Host ""
