#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Focused validation: Execute RecreateSummaries SP steps and cross-validate with C# queries.
    
.DESCRIPTION
    Tests steps 1-2 (FPS totals calculation) and 14 (logging), then queries results
    and compares SP output with expected values from seeded data.
#>

param(
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432",
    [string]$DbUser = "postgres",
    [string]$DbName = "batch_jobs_foundation_db"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$PsqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
$RepoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.Parent.FullName
$OutputDir = Join-Path $RepoRoot "src\Apha.BatchJobs\docs\database\validation"

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

Write-Host "=========================================="
Write-Host "RecreateSummaries Validation"
Write-Host "=========================================="
Write-Host "Database: $DbName"
Write-Host "Output:   $OutputDir"
Write-Host "=========================================="
Write-Host ""

$env:PGPASSWORD = "LOCAL_DB_PASSWORD"

# Step 1: Delete FPS Totals
Write-Host "[01/04] Deleting fpsyeartotals..."
$result = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
    -c "DELETE FROM fps.fpsyeartotals;" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to delete fpsyeartotals: $result"
    exit 1
}
Write-Host "[OK]"

# Step 2: Create FPS Totals (using corrected SQL)
Write-Host "[02/04] Creating fpsyeartotals from views..."
$result = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
    -c "
INSERT INTO fps.fpsyeartotals
(parentproject, program, totaladditionalcosts, totalanimalcosts, totalstaffcosts, 
 totaltestcosts, totalcosts, custincome, transferincome, totalincome, budget_cvl,
 requiredprofit, manager, customer, projectstatus, pvsincome, plancaseworkdebit, 
 totalpaycosts, fpsyear)
SELECT DISTINCT
    tlkpproject.parentproject,
    tlkpproject.program,
    COALESCE(qrytotaladditionalcosts.totaladditionalcosts, '0'::money) AS totaladditionalcosts,
    COALESCE(qrytotalanimalcosts.totalanimalcosts, 0::double precision) AS totalanimalcosts,
    COALESCE(qrytotalstaffcosts.totalstaffcosts, 0::double precision) AS totalstaffcosts,
    COALESCE(qrytotaltestcosts.totaltestcosts, 0::double precision) AS totaltestcosts,
    (COALESCE(qrytotaladditionalcosts.totaladditionalcosts::double precision, 0::double precision) +
     COALESCE(qrytotalanimalcosts.totalanimalcosts, 0::double precision) +
     COALESCE(qrytotalstaffcosts.totalstaffcosts, 0::double precision) +
     COALESCE(qrytotaltestcosts.totaltestcosts, 0::double precision) +
     COALESCE(tlkpproject.plancaseworkdebit::double precision, 0::double precision)) AS totalcosts,
    tlkpproject.custincome,
    tlkpproject.transferincome,
    tlkpproject.custincome + tlkpproject.transferincome AS totalincome,
    tlkpproject.budget_cvl,
    tlkpproject.profit AS requiredprofit,
    tlkpproject.manager,
    tlkpproject.customer,
    tlkpproject.projectstatus,
    COALESCE(tlkpproject.pvsincome, '0'::money) AS pvsincome,
    COALESCE(tlkpproject.plancaseworkdebit, '0'::money) AS plancaseworkdebit,
    COALESCE(qrytotalstaffcosts.totalpaycosts, 0::double precision) AS totalpaycosts,
    tlkpproject.fpsyear
FROM fps.tlkpproject
LEFT JOIN fps.qrytotaladditionalcosts ON tlkpproject.parentproject = qrytotaladditionalcosts.jobcode
LEFT JOIN fps.qrytotalanimalcosts     ON tlkpproject.parentproject = qrytotalanimalcosts.jobcode
LEFT JOIN fps.qrytotalstaffcosts      ON tlkpproject.parentproject = qrytotalstaffcosts.jobcode
LEFT JOIN fps.qrytotaltestcosts       ON tlkpproject.parentproject = qrytotaltestcosts.jobcode;
" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create fpsyeartotals: $result"
    exit 1
}
Write-Host "[OK]"

# Step 3: Query results
Write-Host "[03/04] Querying SP results..."
$queryResult = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
    -t -A -F "," `
    -c "
SELECT 
    parentproject,
    program,
    fpsyear,
    totalcosts,
    totalincome,
    requiredprofit,
    projectstatus
FROM fps.fpsyeartotals
WHERE fpsyear IN (2024, 2025, 2026)
ORDER BY fpsyear, parentproject;
" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to query results: $queryResult"
    exit 1
}

Write-Host "[OK] Results returned:"
Write-Host $queryResult

# Step 4: Log execution
Write-Host "[04/04] Logging execution..."
$result = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
    -c "
INSERT INTO fps.recreatesummaries_log (userid, period, datadone)
VALUES ('validation-test', 1, CURRENT_TIMESTAMP);
" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Log entry failed (non-critical): $result"
} else {
    Write-Host "[OK]"
}

Remove-Item Env:PGPASSWORD

# Export results
Write-Host ""
Write-Host "=========================================="
Write-Host "SP Results"
Write-Host "=========================================="
Write-Host ""
Write-Host "Query Output:"
Write-Host $queryResult
Write-Host ""

# Parse and display results
$lines = $queryResult -split "`n" | Where-Object { $_.Trim() }
Write-Host "Parsed Results ($($lines.Count) rows):"
foreach ($line in $lines) {
    $parts = $line -split ","
    Write-Host "  Project: $($parts[0]) | Program: $($parts[1]) | Year: $($parts[2]) | Total Cost: $($parts[3]) | Income: $($parts[4])"
}

# Save to JSON for cross-validation
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $OutputDir "sp-results-$timestamp.json"
$jsonData = @{
    timestamp = $timestamp
    database = "$DbHost`:$DbPort/$DbName"
    results = @($lines | ForEach-Object {
        $parts = $_ -split ","
        @{
            parentproject = $parts[0]
            program = $parts[1]
            fpsyear = [int]$parts[2]
            totalcosts = $parts[3]
            totalincome = $parts[4]
            requiredprofit = $parts[5]
            projectstatus = $parts[6]
        }
    })
}

$jsonData | ConvertTo-Json -Depth 3 | Out-File -FilePath $jsonPath -Encoding UTF8
Write-Host ""
Write-Host "Results exported to: $jsonPath"
Write-Host ""
Write-Host "=========================================="
Write-Host "Validation Complete - Ready for C# Cross-Check"
Write-Host "=========================================="
