$nativeTargets = Get-Process -ErrorAction SilentlyContinue |
    Where-Object {
        $_.ProcessName -like 'Apha.BatchJobs*' -or
        $_.ProcessName -eq 'BatchJobs'
    }

if ($nativeTargets) {
    $nativeTargets | Stop-Process -Force -ErrorAction SilentlyContinue
}

$dotnetBatchJobsPids = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue |
    Where-Object {
        $_.CommandLine -and $_.CommandLine -match 'Apha\.BatchJobs'
    } |
    Select-Object -ExpandProperty ProcessId

if ($dotnetBatchJobsPids) {
    foreach ($processId in ($dotnetBatchJobsPids | Sort-Object -Unique)) {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    }
}
