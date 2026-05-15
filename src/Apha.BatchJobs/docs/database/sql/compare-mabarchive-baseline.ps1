#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compares two MABArchive baseline snapshot JSON files.

.DESCRIPTION
    Validates per-loader rowCount and rowHash parity for loaders 1..24 and
    prints a concise mismatch report with a non-zero exit code on failure.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaselineJson,

    [Parameter(Mandatory = $true)]
    [string]$CandidateJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $BaselineJson)) {
    throw "Baseline file not found: $BaselineJson"
}

if (-not (Test-Path $CandidateJson)) {
    throw "Candidate file not found: $CandidateJson"
}

$baseline = Get-Content -Path $BaselineJson -Raw | ConvertFrom-Json
$candidate = Get-Content -Path $CandidateJson -Raw | ConvertFrom-Json

$baselineTargets = @($baseline.targets)
$candidateTargets = @($candidate.targets)

if ($baselineTargets.Count -ne 24 -or $candidateTargets.Count -ne 24) {
    throw "Expected 24 loader targets in each file. Baseline=$($baselineTargets.Count), Candidate=$($candidateTargets.Count)."
}

$candidateBySeq = @{}
foreach ($item in $candidateTargets) {
    $candidateBySeq[[int]$item.sequence] = $item
}

$comparisons = @()
foreach ($baseItem in $baselineTargets | Sort-Object sequence) {
    $seq = [int]$baseItem.sequence
    if (-not $candidateBySeq.ContainsKey($seq)) {
        $comparisons += [pscustomobject]@{
            sequence = $seq
            loader = $baseItem.loader
            rowCountMatch = $false
            rowHashMatch = $false
            baselineRowCount = [long]$baseItem.rowCount
            candidateRowCount = $null
            baselineRowHash = [string]$baseItem.rowHash
            candidateRowHash = $null
        }
        continue
    }

    $candItem = $candidateBySeq[$seq]
    $comparisons += [pscustomobject]@{
        sequence = $seq
        loader = [string]$baseItem.loader
        rowCountMatch = ([long]$baseItem.rowCount -eq [long]$candItem.rowCount)
        rowHashMatch = ([string]$baseItem.rowHash -eq [string]$candItem.rowHash)
        baselineRowCount = [long]$baseItem.rowCount
        candidateRowCount = [long]$candItem.rowCount
        baselineRowHash = [string]$baseItem.rowHash
        candidateRowHash = [string]$candItem.rowHash
    }
}

$mismatches = @($comparisons | Where-Object { -not $_.rowCountMatch -or -not $_.rowHashMatch })

Write-Host "=========================================="
Write-Host "MABArchive Snapshot Compare"
Write-Host "=========================================="
Write-Host "Baseline:  $BaselineJson"
Write-Host "Candidate: $CandidateJson"
Write-Host "Loaders compared: $($comparisons.Count)"
Write-Host "Mismatches: $($mismatches.Count)"
Write-Host "=========================================="

if ($mismatches.Count -gt 0) {
    Write-Host ""
    Write-Host "Mismatch details"
    Write-Host "------------------------------------------"
    $mismatches |
        Sort-Object sequence |
        Format-Table sequence, loader, rowCountMatch, rowHashMatch, baselineRowCount, candidateRowCount -AutoSize
    exit 1
}

Write-Host "All loader snapshots match."
exit 0
