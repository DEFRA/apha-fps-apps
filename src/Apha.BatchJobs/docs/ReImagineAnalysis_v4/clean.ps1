$files = @(
    "AphaBatchJobsFoundationV3.Core\AphaBatchJobsFoundationV3.Core.csproj",
    "AphaBatchJobsFoundationV3.Infrastructure\Data\BatchJobDbContext.cs",
    "AphaBatchJobsFoundationV3.Host\Services\CliJobExecutor.cs",
    "AphaBatchJobsFoundationV3.Host\Configuration\CommandLineOptions.cs",
    "AphaBatchJobsFoundationV3.Core\Models\CorrelationContext.cs",
    "AphaBatchJobsFoundationV3.Infrastructure\Services\CorrelationService.cs",
    "Dockerfile",
    "AphaBatchJobsFoundationV3.Infrastructure\Extensions\InfrastructureDependencyInjection.cs",
    "AphaBatchJobsFoundationV3.Host\Program.cs",
    "AphaBatchJobsFoundationV3.Infrastructure\Scheduling\QuartzJobScheduler.cs",
    "AphaBatchJobsFoundationV3.Infrastructure\Scheduling\QuartzJobWrapper.cs",
    "AphaBatchJobsFoundationV3.Host\Services\SchedulerHostedService.cs"
)

foreach ($f in $files) {
    if (Test-Path $f) {
        $content = Get-Content $f
        $clean = @()
        foreach ($line in $content) {
            if ($line -match "\*\*") { break }
            $clean += $line
        }
        while ($clean.Count -gt 0 -and [string]::IsNullOrWhiteSpace($clean[-1])) {
            $clean = $clean[0..($clean.Count-2)]
        }
        $clean | Set-Content $f
        Write-Host "OK: $f"
    }
}
