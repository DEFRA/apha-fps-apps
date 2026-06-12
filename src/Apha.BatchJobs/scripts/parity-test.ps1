param([string[]]$Months = @('0','12'))

$ErrorActionPreference = 'Continue'
$root = 'D:\Users\atos.user8\source\repos\apha-fps-apps-C-Adhoc'
$psql = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$env:PGPASSWORD = 'LOCAL_DB_PASSWORD'
$pgBase = @('-h','localhost','-p','5432','-U','postgres','-d','postgres')

function Invoke-Psql([string]$db, [string]$sql) {
    $args2 = @('-h','localhost','-p','5432','-U','postgres','-d',$db,'-v','ON_ERROR_STOP=1')
    $out = & $psql @args2 -c $sql 2>&1
    if ($LASTEXITCODE -ne 0) { throw "psql failed [$db]: $sql`n$out" }
    return $out
}

function Query-Psql([string]$db, [string]$sql) {
    $args2 = @('-h','localhost','-p','5432','-U','postgres','-d',$db,'-t','-A')
    $out = & $psql @args2 -c $sql 2>&1
    if ($LASTEXITCODE -ne 0) { throw "psql query failed [$db]: $sql`n$out" }
    return ($out | Select-Object -First 1).Trim()
}

function Drop-And-Clone([string]$dbName) {
    Write-Host "  Cloning -> $dbName"
    & $psql @pgBase -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='$dbName' AND pid<>pg_backend_pid();" | Out-Null
    & $psql @pgBase -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='batch_jobs_foundation_db' AND pid<>pg_backend_pid();" | Out-Null
    Start-Sleep -Milliseconds 800
    & $psql @pgBase -c "DROP DATABASE IF EXISTS $dbName;" | Out-Null
    $out = & $psql @pgBase -c "CREATE DATABASE $dbName TEMPLATE batch_jobs_foundation_db;"
    if ($LASTEXITCODE -ne 0) { throw "Failed to create $dbName`: $out" }
    Write-Host "  Created $dbName OK"
}

function Run-Worker([string]$impl, [string]$connDb, [string]$month, [string]$logFile) {
    Write-Host "  Running $impl worker (month=$month, db=$connDb)..."
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:BATCH_JOB_NAME = 'RecreateSummaries'
    $env:BATCH_RUN_MODE = 'Manual'
    $env:ConnectionStrings__FPSConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=LOCAL_DB_PASSWORD;Database=$connDb"
    $env:BatchJobs__RecreateSummariesImplementationMode = $impl
    $env:BATCH_RECREATE_SUMMARIES_MONTH = $month
    $env:BATCH_RECREATE_SUMMARIES_TRIGGERED_BY = 'parity-user'
    & dotnet run --project "$root\src\Apha.BatchJobs\Apha.BatchJobs.Worker\Apha.BatchJobs.Worker.csproj" -- RecreateSummaries *> $logFile
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Worker FAILED for $impl. Last 40 lines:"
        Get-Content $logFile -Tail 40 | Write-Host
        throw "Worker failed for $impl month=$month"
    }
    Write-Host "  $impl worker done (month=$month)"
}

function Get-Fingerprints([string]$db) {
    $tables = @(
        'fps.fpsyeartotals','fps.projectmonth','fps.timecostcalcs',
        'fps.projectmonthcasework','fps.projectmonth2','fps.projectmonth3',
        'fps.projectmonthfinal','fps.period_monthlyoutput',
        'fps.period_proj_subcontract','fps.period_timecostcalcs'
    )
    $result = @()
    foreach ($t in $tables) {
        $cnt = Query-Psql $db "SELECT COUNT(*) FROM $t;"
        $fp  = Query-Psql $db "SELECT md5(COALESCE(string_agg(md5(to_jsonb(x)::text),'' ORDER BY md5(to_jsonb(x)::text)),'')) FROM $t x;"
        $result += [pscustomobject]@{Table=$t; Count=$cnt; FP=$fp}
    }
    return $result
}

$overallPass = $true

foreach ($month in $Months) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  PARITY TEST  Month=$month" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan

    $sqlDb  = 'recreate_cmp_sql'
    $dotDb  = 'recreate_cmp_dotnet'
    $log1   = Join-Path $env:TEMP "parity_sqlfiles_m$month.log"
    $log2   = Join-Path $env:TEMP "parity_dotnet_m$month.log"

    Drop-And-Clone $sqlDb
    Drop-And-Clone $dotDb

    Run-Worker 'SqlFiles' $sqlDb $month $log1
    Run-Worker 'DotNet'   $dotDb $month $log2

    Write-Host "  Comparing fingerprints..."
    $sqlFP = Get-Fingerprints $sqlDb
    $dotFP = Get-Fingerprints $dotDb

    $monthPass = $true
    $rows = foreach ($s in $sqlFP) {
        $d = $dotFP | Where-Object { $_.Table -eq $s.Table }
        $match = ($s.Count -eq $d.Count -and $s.FP -eq $d.FP)
        if (-not $match) { $monthPass = $false; $overallPass = $false }
        [pscustomobject]@{
            Table    = $s.Table
            SqlCount = $s.Count
            DotCount = $d.Count
            Match    = if ($match) { 'OK' } else { 'MISMATCH' }
            SqlFP    = $s.FP.Substring(0,8)+'...'
        }
    }
    $rows | Format-Table -AutoSize | Out-Host

    $sqlLog = Query-Psql $sqlDb "SELECT userid||'|'||period FROM fps.recreatesummaries_log ORDER BY datedone DESC LIMIT 1;"
    $dotLog = Query-Psql $dotDb  "SELECT userid||'|'||period FROM fps.recreatesummaries_log ORDER BY datedone DESC LIMIT 1;"
    Write-Host "  SQL log: $sqlLog"
    Write-Host "  DOT log: $dotLog"

    if ($monthPass) {
        Write-Host "  PARITY_RESULT=PASS (month=$month)" -ForegroundColor Green
    } else {
        Write-Host "  PARITY_RESULT=FAIL (month=$month)" -ForegroundColor Red
    }
}

Write-Host ""
if ($overallPass) {
    Write-Host "ALL MONTHS PASSED" -ForegroundColor Green
} else {
    Write-Host "SOME MONTHS FAILED" -ForegroundColor Red
    exit 1
}
