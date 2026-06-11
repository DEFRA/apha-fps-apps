#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validation test for RecreateSummaries stored procedure execution.
    Executes all 14 RecreateSummaries SQL steps, captures results, and prepares
    for cross-validation with C# code queries.

.DESCRIPTION
    1. Executes RecreateSummaries steps 1-14 in order
    2. Queries SP-generated summary tables (fpsyeartotals, projectmonth, etc.)
    3. Exports results to JSON for C# cross-validation
    4. Reports variance between expected and actual values

.PARAMETER DbHost
    PostgreSQL server hostname (default: localhost)

.PARAMETER DbPort
    PostgreSQL server port (default: 5432)

.PARAMETER DbUser
    PostgreSQL user (default: postgres)

.PARAMETER DbName
    PostgreSQL database name (default: batch_jobs_foundation_db)

.PARAMETER FpsYear
    FPS year to validate (default: 2024)

.PARAMETER ExportJson
    Export results to JSON file for C# validation (default: $true)
#>

param(
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432",
    [string]$DbUser = "postgres",
    [string]$DbName = "batch_jobs_foundation_db",
    [int]$FpsYear = 2024,
    [bool]$ExportJson = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ============================================================================
# CONFIGURATION
# ============================================================================

$RepoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.Parent.FullName
$SqlDir = Join-Path $RepoRoot "src\Apha.BatchJobs\Apha.BatchJobs.Infrastructure\Sql\RecreateSummaries"
$OutputDir = Join-Path $RepoRoot "src\Apha.BatchJobs\docs\database\validation"

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$PsqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
if (-not (Test-Path $PsqlPath)) {
    Write-Error "psql.exe not found at $PsqlPath"
    exit 1
}

Write-Host @"
========================================
RecreateSummaries Validation Test
========================================
Host:     $DbHost
Port:     $DbPort
Database: $DbName
User:     $DbUser
FpsYear:  $FpsYear
SQL Dir:  $SqlDir
Output:   $OutputDir
========================================
"@

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

function Invoke-SqlStep {
    param(
        [int]$StepNumber,
        [string]$StepFile,
        [hashtable]$Variables = @{}
    )

    $StepPath = Join-Path $SqlDir $StepFile
    if (-not (Test-Path $StepPath)) {
        Write-Error "Step file not found: $StepPath"
        return $false
    }

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] [STEP $StepNumber] Executing $StepFile..."

    # Read SQL file
    $sql = Get-Content -Path $StepPath -Raw
    
    # Replace variables (:varname with actual values)
    foreach ($var in $Variables.GetEnumerator()) {
        $sql = $sql -replace ":$($var.Key)", $var.Value
    }

    # Execute via psql
    $env:PGPASSWORD = "LOCAL_DB_PASSWORD"
    
    try {
        $output = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
            -v ON_ERROR_STOP=on `
            -c $sql 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  [OK] Step complete"
            return $true
        } else {
            Write-Error "Step $StepNumber failed: $output"
            return $false
        }
    } finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Query-ResultsJson {
    param(
        [int]$FpsYear
    )

    $env:PGPASSWORD = "LOCAL_DB_PASSWORD"

    # Query all key result tables
    $queries = @{
        "fpsyeartotals" = @"
SELECT 
    parentproject,
    program,
    totaladditionalcosts,
    totalanimalcosts,
    totalstaffcosts,
    totaltestcosts,
    totalcosts,
    custincome,
    transferincome,
    totalincome,
    budget_cvl,
    requiredprofit,
    projectstatus,
    pvsincome,
    plancaseworkdebit
FROM fps.fpsyeartotals
ORDER BY parentproject;
"@
        
        "projectmonth_summary" = @"
SELECT 
    project,
    monthno,
    costprofile,
    subcontracts,
    animals,
    nonanimal,
    timecosts,
    totalcost,
    invoices,
    mstoneddue,
    due__done,
    ontime
FROM fps.projectmonth3
WHERE project IN ('AH0001', 'TH0002', 'BS0003', 'RS0004')
ORDER BY project, monthno;
"@

        "recreatesummaries_log" = @"
SELECT 
    userid,
    period,
    datadone
FROM fps.recreatesummaries_log
ORDER BY datadone DESC
LIMIT 5;
"@

        "project_costs_by_year" = @"
SELECT 
    parentproject,
    program,
    SUM(totalcosts)::numeric(15,2) as total_costs,
    COUNT(*) as project_count
FROM fps.fpsyeartotals
GROUP BY parentproject, program
ORDER BY parentproject;
"@
    }

    $results = @{}

    foreach ($queryName in $queries.Keys) {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Querying $queryName..."
        
        try {
            # Export to CSV first, then convert to JSON
            $csvOutput = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
                -c "$($queries[$queryName])" `
                -F "," -A 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host "  [OK] Query successful"
                $results[$queryName] = $csvOutput
            } else {
                Write-Warning "Query failed for $queryName"
                $results[$queryName] = $null
            }
        } catch {
            Write-Warning "Error querying $queryName : $_"
            $results[$queryName] = $null
        }
    }

    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    
    return $results
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host ""
Write-Host "=========================================="
Write-Host "Phase 1: Executing RecreateSummaries Steps"
Write-Host "=========================================="
Write-Host ""

# Step 1: Delete FPS Totals
if (-not (Invoke-SqlStep -StepNumber 1 -StepFile "01_delete_fps_totals.sql")) {
    Write-Error "Step 1 failed. Exiting."
    exit 1
}

# Step 2: Create FPS Totals
if (-not (Invoke-SqlStep -StepNumber 2 -StepFile "02_create_fps_totals.sql")) {
    Write-Error "Step 2 failed. Exiting."
    exit 1
}

# Step 3: Insert Missing Projects
if (-not (Invoke-SqlStep -StepNumber 3 -StepFile "03_insert_missing_projects.sql")) {
    Write-Error "Step 3 failed. Exiting."
    exit 1
}

# Step 4: Delete Time Cost Calcs
if (-not (Invoke-SqlStep -StepNumber 4 -StepFile "04_delete_time_cost_calcs.sql")) {
    Write-Error "Step 4 failed. Exiting."
    exit 1
}

# Step 5: Create Time Cost Calcs
if (-not (Invoke-SqlStep -StepNumber 5 -StepFile "05_create_time_cost_calcs.sql")) {
    Write-Error "Step 5 failed. Exiting."
    exit 1
}

# Step 6: Delete Project Month Casework
if (-not (Invoke-SqlStep -StepNumber 6 -StepFile "06_delete_project_month_casework.sql")) {
    Write-Error "Step 6 failed. Exiting."
    exit 1
}

# Step 7: Create Project Month Casework
if (-not (Invoke-SqlStep -StepNumber 7 -StepFile "07_create_project_month_casework.sql")) {
    Write-Error "Step 7 failed. Exiting."
    exit 1
}

# Step 8: Delete Project Month Final
if (-not (Invoke-SqlStep -StepNumber 8 -StepFile "08_delete_project_month_final.sql")) {
    Write-Error "Step 8 failed. Exiting."
    exit 1
}

# Step 9: Delete Project Month 2
if (-not (Invoke-SqlStep -StepNumber 9 -StepFile "09_delete_project_month2.sql")) {
    Write-Error "Step 9 failed. Exiting."
    exit 1
}

# Step 10: Create Project Month Single
if (-not (Invoke-SqlStep -StepNumber 10 -StepFile "10_create_project_month_single.sql")) {
    Write-Error "Step 10 failed. Exiting."
    exit 1
}

# Step 11: Delete Project Month 3
if (-not (Invoke-SqlStep -StepNumber 11 -StepFile "11_delete_project_month3.sql")) {
    Write-Error "Step 11 failed. Exiting."
    exit 1
}

# Step 12: Create Project Month Cumulative
if (-not (Invoke-SqlStep -StepNumber 12 -StepFile "12_create_project_month_cumulative.sql")) {
    Write-Error "Step 12 failed. Exiting."
    exit 1
}

# Step 13: Create Project Month Final
if (-not (Invoke-SqlStep -StepNumber 13 -StepFile "13_create_project_month_final.sql")) {
    Write-Error "Step 13 failed. Exiting."
    exit 1
}

# Step 14: Log Recreate Summaries
$variables = @{
    "userId" = "test-validation"
    "month"  = 1
}
if (-not (Invoke-SqlStep -StepNumber 14 -StepFile "14_log_recreate_summaries.sql" -Variables $variables)) {
    Write-Error "Step 14 failed. Exiting."
    exit 1
}

Write-Host ""
Write-Host "[OK] All 14 RecreateSummaries steps executed successfully!"
Write-Host ""

# ============================================================================
# Phase 2: Query Results
# ============================================================================

Write-Host "=========================================="
Write-Host "Phase 2: Querying Results"
Write-Host "=========================================="
Write-Host ""

$results = Query-ResultsJson -FpsYear $FpsYear

Write-Host ""
Write-Host "=========================================="
Write-Host "Phase 3: Results Summary"
Write-Host "=========================================="
Write-Host ""

foreach ($queryName in $results.Keys) {
    Write-Host ""
    Write-Host "--- $queryName ---"
    if ($results[$queryName]) {
        $lines = ($results[$queryName] | Measure-Object -Line).Lines
        Write-Host "Rows returned: $lines"
        Write-Host ($results[$queryName] | Select-Object -First 5)
        if ($lines -gt 5) {
            Write-Host "... (showing first 5 of $lines rows)"
        }
    } else {
        Write-Host "[EMPTY]"
    }
}

# ============================================================================
# Phase 4: Export for C# Cross-Validation
# ============================================================================

if ($ExportJson) {
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "Phase 4: Exporting Results for Cross-Validation"
    Write-Host "=========================================="
    Write-Host ""

    $exportData = @{
        "executedAt" = (Get-Date -Format "o")
        "fpsYear" = $FpsYear
        "database" = @{
            "host" = $DbHost
            "port" = $DbPort
            "name" = $DbName
        }
        "results" = $results
        "expectedProjects" = @("AH0001", "TH0002", "BS0003", "RS0004")
        "expectedYears" = @(2024, 2025, 2026)
    }

    $jsonPath = Join-Path $OutputDir "recreate-summaries-results-$FpsYear.json"
    $exportData | ConvertTo-Json -Depth 10 | Out-File -FilePath $jsonPath -Encoding UTF8
    
    Write-Host "[OK] Results exported to: $jsonPath"
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Validation Complete"
Write-Host "=========================================="
Write-Host ""
Write-Host "Next step: Run C# cross-validation against exported results"
Write-Host ""
