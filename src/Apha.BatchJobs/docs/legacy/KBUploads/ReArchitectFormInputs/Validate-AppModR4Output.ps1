param(
    [Parameter(Mandatory = $true)]
    [string]$OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $OutputRoot -PathType Container)) {
    throw "Output folder not found: $OutputRoot"
}

$requiredProcedureNames = @(
    "sp_CreateProjectMonthCasework",
    "sp_CreateTimeCostCalcs",
    "sp_DeleteProjectMonth3",
    "sp_DeleteProjectMonthCasework",
    "sp_DeleteProjectMonthFinal",
    "sp_InsertMissingProjects",
    "sp_RecreateSummaries",
    "sp_deleteProjectMonth2",
    "sp_deleteTimeCostCalcs",
    "sp_qryJobMonthCum",
    "sp_qryJobMonth_Final",
    "sp_qryJobMonth_Single",
    "usp_LogRecreateSummaries",
    "usp_Refresh_Period_MO",
    "usp_Refresh_Period_PSC",
    "usp_Refresh_Period_TCC",
    "spResetSendEmail",
    "spSendProgramManagerReportEmail",
    "spSendProgramReportNotification",
    "spSendProjectManagerReportEmail",
    "spSendProjectReportNotification",
    "spSendRCManagerReportEmail",
    "spSendRCReportNotification",
    "spSendReportEmails_Manual"
)

$requiredMarkers = @(
    "public sealed class AdhocRecreateSummariesJob : IAdhocJob",
    "services.AddSingleton<IAdhocJob>",
    "Step 1/17",
    "Step 17/17",
    "periodLocked"
)

$forbiddenPatterns = @(
    'Key improvements made',
    'Key Improvements Made',
    'TODO',
    'placeholder',
    'recommendation',
    '```',
    '^# '
)

function Get-CsFiles {
    param([string]$Root)
    Get-ChildItem -LiteralPath $Root -Recurse -File -Filter "*.cs"
}

function Find-AnyFileByName {
    param(
        [System.IO.FileInfo[]]$Files,
        [string]$Name
    )
    $Files | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
}

$csFiles = Get-CsFiles -Root $OutputRoot
if (-not $csFiles -or $csFiles.Count -eq 0) {
    throw "No C# files found under output root: $OutputRoot"
}

$errors = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]

# Required files/classes
$orchestratorFile = Find-AnyFileByName -Files $csFiles -Name "AdhocRecreateSummariesJob.cs"
if (-not $orchestratorFile) {
    $errors.Add("Missing required file: AdhocRecreateSummariesJob.cs")
}

$diFile = $csFiles | Where-Object {
    $_.Name -match "DependencyInjection|ServiceCollectionExtensions"
} | Select-Object -First 1
if (-not $diFile) {
    $errors.Add("Missing DI file: expected DependencyInjection*.cs or ServiceCollectionExtensions*.cs")
}

# Read all C# content once for whole-output checks
$allText = ($csFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"

# Required markers in whole output
foreach ($marker in $requiredMarkers) {
    if ($allText -notmatch [regex]::Escape($marker)) {
        $errors.Add("Missing required marker: $marker")
    }
}

# Forbidden prose/content checks
foreach ($file in $csFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8

    foreach ($pattern in $forbiddenPatterns) {
        if ($content -match $pattern) {
            $errors.Add("Forbidden pattern '$pattern' found in $($file.FullName)")
        }
    }
}

# Procedure coverage checks (all 24 names should appear somewhere in generated C#)
$coveredProcedures = New-Object System.Collections.Generic.List[string]
$missingProcedures = New-Object System.Collections.Generic.List[string]

foreach ($proc in $requiredProcedureNames) {
    if ($allText -match [regex]::Escape($proc)) {
        $coveredProcedures.Add($proc)
    }
    else {
        $missingProcedures.Add($proc)
    }
}

if ($missingProcedures.Count -gt 0) {
    $errors.Add("Procedure coverage missing: $($missingProcedures.Count)/24 not found")
}

# Orchestrator specific checks
if ($orchestratorFile) {
    $orchestratorText = Get-Content -LiteralPath $orchestratorFile.FullName -Raw -Encoding UTF8

    if ($orchestratorText -notmatch "SQL Source:\s*") {
        $warnings.Add("No 'SQL Source: <procedure>' traceability comments found in orchestrator")
    }

    if ($orchestratorText -notmatch "month\s*<\s*1|month\s*>\s*12") {
        $errors.Add("Orchestrator missing explicit month range validation (1..12)")
    }
}

$coveragePct = [math]::Round(($coveredProcedures.Count / 24.0) * 100, 2)

Write-Host "========================================"
Write-Host "AppMod R4 Output Validation Report"
Write-Host "========================================"
Write-Host "Output Root           : $OutputRoot"
Write-Host "C# Files              : $($csFiles.Count)"
Write-Host "Procedure Coverage    : $($coveredProcedures.Count)/24 (${coveragePct}%)"
Write-Host "Errors                : $($errors.Count)"
Write-Host "Warnings              : $($warnings.Count)"
Write-Host ""

if ($missingProcedures.Count -gt 0) {
    Write-Host "Missing Procedures:" -ForegroundColor Yellow
    $missingProcedures | ForEach-Object { Write-Host " - $_" }
    Write-Host ""
}

if ($warnings.Count -gt 0) {
    Write-Host "Warnings:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host " - $_" }
    Write-Host ""
}

if ($errors.Count -gt 0) {
    Write-Host "Errors:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host " - $_" }
    Write-Host ""
    Write-Host "RESULT: FAIL" -ForegroundColor Red
    exit 1
}

Write-Host "RESULT: PASS" -ForegroundColor Green
exit 0
