$scriptPath = Join-Path $PSScriptRoot "src\Apha.BatchJobs\test-locally.ps1"

if (-not (Test-Path $scriptPath)) {
    Write-Error "Could not find target script at $scriptPath"
    exit 1
}

& $scriptPath @args
exit $LASTEXITCODE
