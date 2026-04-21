# Complete Test Matrix - All Scenarios

## 📊 Test Scenarios Overview

| # | Job Name | Mode | Database | Environment | Expected Duration |
|---|----------|------|----------|-------------|-------------------|
| 1 | HealthCheck | NoDb | In-Memory | Demo | ~4s |
| 2 | HealthCheck | WithDb | PostgreSQL | Development | ~17s |
| 3 | ScheduleJobs | NoDb | In-Memory | Demo | ~5s |
| 4 | ScheduleJobs | WithDb | PostgreSQL | Development | ~18s |
| 5 | FECProcess | NoDb | In-Memory | Demo | ~5s |
| 6 | FECProcess | WithDb | PostgreSQL | Development | ~18s |

**Note:** RecreateSummaries excluded as requested

---

## 🖥️ VM Test Commands

### Prerequisites:
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
```

---

## Test 1: HealthCheck - NoDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- HealthCheck
```

## Test 2: HealthCheck - WithDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- HealthCheck
```

## Test 3: ScheduleJobs - NoDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- ScheduleJobs
```

## Test 4: ScheduleJobs - WithDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- ScheduleJobs
```

## Test 5: FECProcess - NoDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- FECProcess
```

## Test 6: FECProcess - WithDb
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- FECProcess
```

---

## 🚀 Run All Tests (Copy-Paste Script)

```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running All Batch Job Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: HealthCheck - NoDb
Write-Host "Test 1: HealthCheck - NoDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- HealthCheck
Write-Host ""

# Test 2: HealthCheck - WithDb
Write-Host "Test 2: HealthCheck - WithDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- HealthCheck
Write-Host ""

# Test 3: ScheduleJobs - NoDb
Write-Host "Test 3: ScheduleJobs - NoDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- ScheduleJobs
Write-Host ""

# Test 4: ScheduleJobs - WithDb
Write-Host "Test 4: ScheduleJobs - WithDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- ScheduleJobs
Write-Host ""

# Test 5: FECProcess - NoDb
Write-Host "Test 5: FECProcess - NoDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- FECProcess
Write-Host ""

# Test 6: FECProcess - WithDb
Write-Host "Test 6: FECProcess - WithDb" -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run -- FECProcess
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "  All Tests Completed" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
```

---

## ✅ Success Criteria

Each test should show:
- ✅ `Outcome=Succeeded`
- ✅ `ExitCode=0`
- ✅ `Status=Completed`
- ✅ `GracefulShutdownCompleted=True`

---

## 📈 Expected Results Summary

| Job | NoDb Result | WithDb Result |
|-----|-------------|---------------|
| HealthCheck | ✅ Should pass | ✅ Should pass |
| ScheduleJobs | ✅ Should pass | ✅ Should pass |
| FECProcess | ✅ Should pass | ✅ Should pass |

---

## 🔍 Verification Commands

```powershell
# Check if PostgreSQL is running (for WithDb tests)
Get-Service -Name "*postgres*"

# Verify appsettings.local.json exists (for WithDb tests)
Test-Path appsettings.local.json

# Check last test exit code
$LASTEXITCODE
```

---

## 📝 Test Log Template

```
Test #: [Job Name] - [Mode]
Started: [Time]
Ended: [Time]
Duration: [Seconds]
Status: [Completed/Failed]
Exit Code: [0/1/2/3/4/5]
Notes: [Any observations]
```
