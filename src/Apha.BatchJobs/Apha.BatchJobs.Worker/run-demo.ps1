# Batch Jobs Demo Script
# Run from: D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker\

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  APHA Batch Jobs Demo Launcher" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Select Demo Mode:" -ForegroundColor Yellow
Write-Host "  1. Demo Mode (NoDb) - Runs without PostgreSQL" -ForegroundColor Green
Write-Host "  2. Production Mode (WithDb) - Requires PostgreSQL connection" -ForegroundColor Magenta
Write-Host "  3. Development Mode (WithDb) - Local PostgreSQL" -ForegroundColor Blue
Write-Host ""

$choice = Read-Host "Enter your choice (1, 2, or 3)"

$jobName = Read-Host "Enter job name (default: HealthCheck)"
if ([string]::IsNullOrWhiteSpace($jobName)) {
    $jobName = "HealthCheck"
}

switch ($choice) {
    "1" {
        Write-Host "`n≡ƒÜÇ Starting Demo Mode (NoDb)..." -ForegroundColor Green
        $env:ASPNETCORE_ENVIRONMENT = "Demo"
        Write-Host "   Environment: Demo" -ForegroundColor Gray
        Write-Host "   Database: In-Memory (No PostgreSQL required)" -ForegroundColor Gray
    }
    "2" {
        Write-Host "`n≡ƒÜÇ Starting Production Mode (WithDb)..." -ForegroundColor Magenta
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        Write-Host "   Environment: Production" -ForegroundColor Gray
        Write-Host "   Database: PostgreSQL (Connection string required)" -ForegroundColor Gray
    }
    "3" {
        Write-Host "`n≡ƒÜÇ Starting Development Mode (WithDb)..." -ForegroundColor Blue
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        Write-Host "   Environment: Development" -ForegroundColor Gray
        Write-Host "   Database: Local PostgreSQL (localhost:5432)" -ForegroundColor Gray
    }
    default {
        Write-Host "Invalid choice. Exiting." -ForegroundColor Red
        exit 1
    }
}

Write-Host "   Job Name: $jobName" -ForegroundColor Gray
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Run from current Worker directory
dotnet run -- $jobName
