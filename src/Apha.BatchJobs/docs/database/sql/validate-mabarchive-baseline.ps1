#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Captures deterministic baseline snapshots for MABArchive loader targets.

.DESCRIPTION
    Produces per-loader row counts and canonical row hashes for the 24 target
    tables loaded by the MABArchive year-load pipeline. Results are exported
    to docs/database/validation for parity tracking.
#>

param(
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432",
    [string]$DbUser = "postgres",
    [string]$DbName = "batch_jobs_foundation_db",
    [int]$Year = 2025,
    [string]$PsqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $PsqlPath)) {
    throw "psql.exe not found at: $PsqlPath"
}

$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.Parent.FullName
$outputDir = Join-Path $repoRoot "src\Apha.BatchJobs\docs\database\validation"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$loaderTargets = @(
    @{ Sequence = 1; Loader = "my_tlkpprogram"; Table = "mabarchive.my_tlkpprogram"; Filter = "year = :year" },
    @{ Sequence = 2; Loader = "g_tlkpproject"; Table = "mabarchive.g_tlkpproject"; Filter = "parentproject IN (SELECT DISTINCT parentproject FROM fps.tlkpproject WHERE fpsyear = :year)" },
    @{ Sequence = 3; Loader = "my_tlkpproject"; Table = "mabarchive.my_tlkpproject"; Filter = "year = :year" },
    @{ Sequence = 4; Loader = "my_fpsyeartotals"; Table = "mabarchive.my_fpsyeartotals"; Filter = "year = :year" },
    @{ Sequence = 5; Loader = "my_monthlyoutput"; Table = "mabarchive.my_monthlyoutput"; Filter = "year = :year" },
    @{ Sequence = 6; Loader = "my_monthlytime"; Table = "mabarchive.my_monthlytime"; Filter = "year = :year" },
    @{ Sequence = 7; Loader = "my_proj_invoice"; Table = "mabarchive.my_proj_invoice"; Filter = "year = :year" },
    @{ Sequence = 8; Loader = "my_proj_subcontract"; Table = "mabarchive.my_proj_subcontract"; Filter = "year = :year" },
    @{ Sequence = 9; Loader = "my_projectmonthfinal"; Table = "mabarchive.my_projectmonthfinal"; Filter = "year = :year" },
    @{ Sequence = 10; Loader = "my_tbladditionalcosts"; Table = "mabarchive.my_tbladditionalcosts"; Filter = "year = :year" },
    @{ Sequence = 11; Loader = "my_tblanimalreq"; Table = "mabarchive.my_tblanimalreq"; Filter = "year = :year" },
    @{ Sequence = 12; Loader = "my_tblcontract"; Table = "mabarchive.my_tblcontract"; Filter = "year = :year" },
    @{ Sequence = 13; Loader = "my_tblstaffjob"; Table = "mabarchive.my_tblstaffjob"; Filter = "year = :year" },
    @{ Sequence = 14; Loader = "my_timecostcalcs"; Table = "mabarchive.my_timecostcalcs"; Filter = "year = :year" },
    @{ Sequence = 15; Loader = "my_tlkptestreqmt"; Table = "mabarchive.my_tlkptestreqmt"; Filter = "year = :year" },
    @{ Sequence = 16; Loader = "tlkpyear"; Table = "mabarchive.tlkpyear"; Filter = "year = :year" },
    @{ Sequence = 17; Loader = "my_workgroupgrade"; Table = "mabarchive.my_workgroupgrade"; Filter = "year = :year" },
    @{ Sequence = 18; Loader = "my_profitcentregrade"; Table = "mabarchive.my_profitcentregrade"; Filter = "year = :year" },
    @{ Sequence = 19; Loader = "my_tblprofitcentre"; Table = "mabarchive.my_tblprofitcentre"; Filter = "year = :year" },
    @{ Sequence = 20; Loader = "my_testorproduct"; Table = "mabarchive.my_testorproduct"; Filter = "year = :year" },
    @{ Sequence = 21; Loader = "my_staff"; Table = "mabarchive.my_staff"; Filter = "year = :year" },
    @{ Sequence = 22; Loader = "my_workgroup"; Table = "mabarchive.my_workgroup"; Filter = "year = :year" },
    @{ Sequence = 23; Loader = "my_tblanimals"; Table = "mabarchive.my_tblanimals"; Filter = "year = :year" },
    @{ Sequence = 24; Loader = "my_tlkpproject_all"; Table = "mabarchive.my_tlkpproject_all"; Filter = "year = :year" }
)

Write-Host "=========================================="
Write-Host "MABArchive Baseline Snapshot"
Write-Host "=========================================="
Write-Host "Database: $DbHost`:$DbPort/$DbName"
Write-Host "Year:     $Year"
Write-Host "Loaders:  $($loaderTargets.Count)"
Write-Host "=========================================="

$rows = @()
$env:PGPASSWORD = "LOCAL_DB_PASSWORD"

try {
    foreach ($target in $loaderTargets) {
        $filterSql = $target.Filter.Replace(":year", $Year)

        $snapshotSql = @"
WITH filtered AS (
    SELECT to_jsonb(t) AS row_json
    FROM $($target.Table) AS t
    WHERE $filterSql
),
hashed AS (
    SELECT md5(row_json::text) AS row_hash
    FROM filtered
)
SELECT
    $($target.Sequence) AS sequence,
    '$($target.Loader)' AS loader_name,
    '$($target.Table)' AS table_name,
    (SELECT COUNT(*) FROM filtered) AS row_count,
    COALESCE(
        encode(digest(convert_to(COALESCE(string_agg(row_hash, ',' ORDER BY row_hash), ''), 'UTF8'), 'sha256'), 'hex'),
        repeat('0', 64)
    ) AS table_hash
FROM hashed;
"@

        $csvLine = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName `
            -v "ON_ERROR_STOP=1" `
            -t -A -F "," `
            -c $snapshotSql 2>&1

        if ($LASTEXITCODE -ne 0) {
            throw "Snapshot query failed for loader '$($target.Loader)': $csvLine"
        }

        $line = @($csvLine | ForEach-Object { "$_.Trim()" }) |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -First 1
        if (-not $line) {
            throw "No snapshot output returned for loader '$($target.Loader)'."
        }

        $parts = "$line" -split ","
        if ($parts.Count -lt 5) {
            throw "Unexpected snapshot output format for loader '$($target.Loader)': $line"
        }

        $row = [ordered]@{
            sequence = [int]$parts[0]
            loader = $parts[1]
            table = $parts[2]
            rowCount = [long]$parts[3]
            rowHash = $parts[4]
        }
        $rows += [pscustomobject]$row
    }
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

$orderedRows = $rows | Sort-Object sequence

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputPath = Join-Path $outputDir "mabarchive-baseline-$timestamp.json"

$payload = [ordered]@{
    executedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    database = "$DbHost`:$DbPort/$DbName"
    year = $Year
    snapshotFormat = "sha256(string_agg(md5(to_jsonb(row)::text) ordered by md5))"
    targets = $orderedRows
}

$payload | ConvertTo-Json -Depth 5 | Out-File -FilePath $outputPath -Encoding utf8

Write-Host ""
Write-Host "Per-loader baseline snapshot"
Write-Host "------------------------------------------"
$orderedRows | Format-Table sequence, loader, rowCount, rowHash -AutoSize

Write-Host ""
Write-Host "Baseline snapshot exported: $outputPath"
Write-Host "Completed successfully."
