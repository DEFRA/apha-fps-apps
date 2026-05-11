# ============================================================================
# Orchestration Script: Flush & Reseed Database
# ============================================================================
# Purpose: Idempotent local database reset with test/seed data
#          Respects FK constraints and runs scripts in correct order
#
# Usage:
#   .\reseed-local-db.ps1
#
# Requirements:
#   - PostgreSQL 16+ installed at C:\Program Files\PostgreSQL\16\bin\psql.exe
#   - batch_jobs_foundation_db database created and accessible
#   - Scripts in src/Apha.BatchJobs/docs/database/sql/
#
# ============================================================================

param(
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432",
    [string]$DbUser = "postgres",
    [string]$DbName = "batch_jobs_foundation_db"
)

$ErrorActionPreference = "Stop"

$PsqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
$RepoRoot = (Get-Item -Path $PSScriptRoot).Parent.Parent.Parent.Parent

# Validate psql exists
if (-not (Test-Path $PsqlPath)) {
    Write-Error "PostgreSQL not found at $PsqlPath"
    exit 1
}

# Script paths
$FlushScript = Join-Path $RepoRoot "src\Apha.BatchJobs\docs\database\sql\00-flush-test-data.sql"
$SeedScript = Join-Path $RepoRoot "src\Apha.BatchJobs\docs\database\sql\seed-combined-fps-mabarchive.sql"

# Validate scripts exist
@($FlushScript, $SeedScript) | ForEach-Object {
    if (-not (Test-Path $_)) {
        Write-Error "Script not found: $_"
        exit 1
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Reseed Orchestration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Host:     $DbHost"
Write-Host "Port:     $DbPort"
Write-Host "Database: $DbName"
Write-Host "User:     $DbUser"
Write-Host ""

# Step 1: Flush existing test data
Write-Host "[1/2] Flushing existing test data..." -ForegroundColor Yellow
try {
    & $PsqlPath `
        -h $DbHost `
        -p $DbPort `
        -U $DbUser `
        -d $DbName `
        -f $FlushScript `
        2>&1 | Select-Object -Last 20

    Write-Host "✓ Flush complete" -ForegroundColor Green
}
catch {
    Write-Error "Flush failed: $_"
    exit 1
}

Write-Host ""

# Step 2: Reseed with combined data
Write-Host "[2/2] Reseeding with combined FPS + MABArchive data..." -ForegroundColor Yellow
try {
    & $PsqlPath `
        -h $DbHost `
        -p $DbPort `
        -U $DbUser `
        -d $DbName `
        -f $SeedScript `
        2>&1 | Select-Object -Last 20

    Write-Host "✓ Reseed complete" -ForegroundColor Green
}
catch {
    Write-Error "Reseed failed: $_"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Database reset successful!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Data loaded:" -ForegroundColor Cyan
Write-Host "  • 12 parent projects (4 programmes × 3 years)"
Write-Host "  • 48+ milestones (on-time, late, pending)"
Write-Host "  • Monthly outputs, invoices, subcontracts"
Write-Host "  • MABArchive baseline totals & snapshots"
Write-Host ""
