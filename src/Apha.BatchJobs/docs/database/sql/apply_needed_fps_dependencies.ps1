$ErrorActionPreference = 'Continue'
$env:PGPASSWORD = 'LOCAL_DB_PASSWORD'
$psql = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
Set-Location 'D:\Users\atos.user8\source\repos\apha-fps-apps-B-ScheduledJobs'

function Table-Exists([string]$name) {
    $r = & $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -t -A -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='fps' AND table_name='$name';"
    return ([int]$r.Trim()) -gt 0
}

function View-Exists([string]$name) {
    $r = & $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -t -A -c "SELECT COUNT(*) FROM information_schema.views WHERE table_schema='fps' AND table_name='$name';"
    return ([int]$r.Trim()) -gt 0
}

function Ensure-Table-From-File([string]$name) {
    if (Table-Exists $name) {
        Write-Output "OK:$name"
        return
    }

    $f = Join-Path 'src/Apha.BatchJobs/docs/database/dbscript/schemas/01fps/01tables' ($name + '.sql')
    if (-not (Test-Path $f)) {
        Write-Output "NO_FILE:$name"
        return
    }

    & $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -v ON_ERROR_STOP=1 -f $f *> $null
    if ($LASTEXITCODE -eq 0 -and (Table-Exists $name)) {
        Write-Output "CREATED:$name"
    }
    else {
        Write-Output "FAILED:$name"
    }
}

function Ensure-View-From-File([string]$name) {
    if (View-Exists $name) {
        Write-Output "OK_VIEW:$name"
        return
    }

    $f = Join-Path 'src/Apha.BatchJobs/docs/database/dbscript/schemas/01fps/04views' ($name + '.sql')
    if (-not (Test-Path $f)) {
        Write-Output "NO_VIEW_FILE:$name"
        return
    }

    & $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -v ON_ERROR_STOP=1 -f $f *> $null
    if ($LASTEXITCODE -eq 0 -and (View-Exists $name)) {
        Write-Output "CREATED_VIEW:$name"
    }
    else {
        Write-Output "FAILED_VIEW:$name"
    }
}

& $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -c "CREATE SCHEMA IF NOT EXISTS fps; CREATE EXTENSION IF NOT EXISTS citext;" *> $null

$chain = @(
    'tblstatus',
    'tlkpcustomer',
    'tbldisease',
    'tlkpaccountcode',
    'tlkpprojectgroup',
    'tlkpsubaccount',
    'tlkpprogram',
    'tblcontract',
    'tblkpprofitcentre',
    'grade',
    'costcentre',
    'workgroup',
    'divisiongrade',
    'profitcentregrade',
    'workgroupgrade',
    'tblemployee',
    'tblwgemployee',
    'testorproduct',
    'tlkpproject',
    'tlkptestreqmt',
    'tlkptestcapability',
    'timecodevalid',
    'fpsyeartotals',
    'monthlyoutput',
    'monthlytime',
    'proj_invoice',
    'proj_subcontract',
    'projectmonthfinal',
    'tbladditionalcosts',
    'tblanimalreq',
    'tblstaffjob',
    'tbldb_variables',
    'timecostcalcs',
    'tblanimals'
)

foreach ($t in $chain) {
    Ensure-Table-From-File $t
}

if (-not (Table-Exists 'tblyearmaster')) {
    & $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -v ON_ERROR_STOP=1 -c "CREATE TABLE fps.tblyearmaster (fpsyear integer NOT NULL, fpsyearcode varchar(20) NOT NULL, yearstatus varchar(10), remarks text, active boolean NOT NULL DEFAULT true, createdby varchar(100), createdon timestamp without time zone DEFAULT now(), CONSTRAINT pk_tblyearmaster PRIMARY KEY (fpsyear), CONSTRAINT uq_tblyearmaster_fpsyearcode UNIQUE (fpsyearcode));" *> $null
}
if (Table-Exists 'tblyearmaster') { Write-Output 'CREATED:tblyearmaster' } else { Write-Output 'FAILED:tblyearmaster' }

$views = @(
    'qrytotaladditionalcosts',
    'qrytotalanimalcosts',
    'qrytotalstaffcosts',
    'qrytotaltestcosts'
)

foreach ($v in $views) {
    Ensure-View-From-File $v
}

$required = @(
    'tblyearmaster',
    'tlkpprogram',
    'tlkpproject',
    'fpsyeartotals',
    'monthlyoutput',
    'monthlytime',
    'proj_invoice',
    'proj_subcontract',
    'projectmonthfinal',
    'tbladditionalcosts',
    'tblanimalreq',
    'tblcontract',
    'tblstaffjob',
    'timecostcalcs',
    'tlkptestreqmt',
    'tbldb_variables',
    'workgroupgrade',
    'profitcentregrade',
    'tblkpprofitcentre',
    'testorproduct',
    'tblwgemployee',
    'tblemployee',
    'workgroup',
    'tblanimals'
)

Write-Output '--- FINAL TABLE STATUS ---'
foreach ($t in $required) {
    if (Table-Exists $t) { Write-Output "OK:$t" } else { Write-Output "MISSING:$t" }
}

Write-Output '--- FINAL VIEW STATUS ---'
foreach ($v in $views) {
    if (View-Exists $v) { Write-Output "OK_VIEW:$v" } else { Write-Output "MISSING_VIEW:$v" }
}
