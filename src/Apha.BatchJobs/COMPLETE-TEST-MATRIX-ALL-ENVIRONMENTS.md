# Complete Test Matrix - All Environments

## Test Matrix Overview

This document provides a comprehensive test matrix covering **both VM and Container environments** for the Apha Batch Jobs Worker.

### Quick Copy/Paste Command Matrix (Consolidated)

#### VM (PowerShell)

```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker

# 1) Demo - HealthCheck
$env:ASPNETCORE_ENVIRONMENT="Demo"; $env:DOTNET_ENVIRONMENT="Demo"; dotnet run -- HealthCheck

# 2) Development - HealthCheck
$env:ASPNETCORE_ENVIRONMENT="Development"; $env:DOTNET_ENVIRONMENT="Development"; dotnet run -- HealthCheck

# 3) Demo - ScheduleJobs
$env:ASPNETCORE_ENVIRONMENT="Demo"; $env:DOTNET_ENVIRONMENT="Demo"; dotnet run -- ScheduleJobs

# 4) Development - ScheduleJobs
$env:ASPNETCORE_ENVIRONMENT="Development"; $env:DOTNET_ENVIRONMENT="Development"; dotnet run -- ScheduleJobs

# 5) Demo - FECProcess
$env:ASPNETCORE_ENVIRONMENT="Demo"; $env:DOTNET_ENVIRONMENT="Demo"; dotnet run -- FECProcess

# 6) Development - FECProcess
$env:ASPNETCORE_ENVIRONMENT="Development"; $env:DOTNET_ENVIRONMENT="Development"; dotnet run -- FECProcess
```

#### Container (Bash, same approach for NoDb and WithDb)

```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs

# 7) NoDb - HealthCheck
./test-locally.sh --docker-profile nodb --job HealthCheck --no-prompt

# 8) WithDb - HealthCheck
./test-locally.sh --docker-profile withdb --job HealthCheck --no-prompt

# 9) NoDb - ScheduleJobs
./test-locally.sh --docker-profile nodb --job ScheduleJobs --no-prompt

# 10) WithDb - ScheduleJobs
./test-locally.sh --docker-profile withdb --job ScheduleJobs --no-prompt

# 11) NoDb - FECProcess
./test-locally.sh --docker-profile nodb --job FECProcess --no-prompt

# 12) WithDb - FECProcess
./test-locally.sh --docker-profile withdb --job FECProcess --no-prompt
```

---

## 📊 Test Scenario Matrix

| # | Environment | Job | Mode | Duration (Est.) | Exit Code | Status |
|---|-------------|-----|------|-----------------|-----------|--------|
| **VM Tests** |
| 1 | VM | HealthCheck | NoDb | ~6s | 0 | ✅ PASS |
| 2 | VM | HealthCheck | WithDb | ~20s | 0 | ✅ PASS |
| 3 | VM | ScheduleJobs | NoDb | ~5s | 0 | ✅ PASS |
| 4 | VM | ScheduleJobs | WithDb | ~16s | 0 | ✅ PASS |
| 5 | VM | FECProcess | NoDb | ~2s | 0 | ✅ PASS |
| 6 | VM | FECProcess | WithDb | ~10s | 0 | ✅ PASS |
| **Container Tests (GitHub Codespaces)** |
| 7 | Container | HealthCheck | NoDb | ~6s | 0 | ✅ Ready |
| 8 | Container | HealthCheck | WithDb | ~20s | 0 | ✅ Ready |
| 9 | Container | ScheduleJobs | NoDb | ~5s | 0 | ✅ Ready |
| 10 | Container | ScheduleJobs | WithDb | ~16s | 0 | ✅ Ready |
| 11 | Container | FECProcess | NoDb | ~2s | 0 | ✅ Ready |
| 12 | Container | FECProcess | WithDb | ~10s | 0 | ✅ Ready |

**Total Tests:** 12 (6 VM ✅ Complete, 6 Container ✅ Ready)

---

## 🖥️ VM Environment Tests (Windows Server)

### Prerequisites
- ✅ .NET 10 SDK installed
- ✅ PostgreSQL 16 running on localhost:5432
- ✅ Database: `batchjobs` (schema: `operational`)
- ✅ `appsettings.local.json` configured with connection string
- ✅ Working directory: `D:\...\Apha.BatchJobs\Apha.BatchJobs.Worker`

---

### Test 1: VM - HealthCheck NoDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- HealthCheck
```

**Expected Output:**
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~6 seconds
- Exit Code: 0
- No database SQL commands in logs

---

### Test 2: VM - HealthCheck WithDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- HealthCheck
```

**Expected Output:**
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~20 seconds
- Exit Code: 0
- Database operations visible (lock acquisition, execution records)

---

### Test 3: VM - ScheduleJobs NoDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- ScheduleJobs
```

**Expected Output:**
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~5 seconds
- Exit Code: 0
- Message: Foundation layer placeholder

---

### Test 4: VM - ScheduleJobs WithDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- ScheduleJobs
```

**Expected Output:**
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~16 seconds
- Exit Code: 0
- Auto-creates job master record if not exists

---

### Test 5: VM - FECProcess NoDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- FECProcess
```

**Expected Output:**
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~2 seconds
- Exit Code: 0
- Message: Foundation layer placeholder

---

### Test 6: VM - FECProcess WithDb
```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- FECProcess
```

**Expected Output:**
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~10 seconds
- Exit Code: 0
- Database operations completed

---

## 🐳 Container Environment Tests (GitHub Codespaces)

### Prerequisites
- ✅ GitHub Codespaces environment
- ✅ Docker and docker-compose available
- ✅ Repository cloned: `DEFRA/apha-fps-apps` (branch: `A-Foundation`)
- ✅ Working directory: `/workspaces/apha-fps-apps/src/Apha.BatchJobs`

---

### Test 7: Container - HealthCheck NoDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile nodb --job HealthCheck --no-prompt
```

**Expected Output:**
- Container builds successfully
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~6 seconds
- Exit Code: 0
- Container stops cleanly

---

### Test 8: Container - HealthCheck WithDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile withdb --job HealthCheck --no-prompt
```

**Expected Output:**
- PostgreSQL container starts and becomes healthy
- Database initialized with schema
- Batch jobs container connects to database
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~20 seconds
- Exit Code: 0
- Both containers stop cleanly

---

### Test 9: Container - ScheduleJobs NoDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile nodb --job ScheduleJobs --no-prompt
```

**Expected Output:**
- Container runs ScheduleJobs
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~5 seconds
- Exit Code: 0

---

### Test 10: Container - ScheduleJobs WithDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile withdb --job ScheduleJobs --no-prompt
```

**Expected Output:**
- PostgreSQL container running
- ScheduleJobs executes successfully
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~16 seconds
- Exit Code: 0

---

### Test 11: Container - FECProcess NoDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile nodb --job FECProcess --no-prompt
```

**Expected Output:**
- Container runs FECProcess
- Environment: Demo
- Execution Mode: NoDb (In-Memory)
- Duration: ~2 seconds
- Exit Code: 0

---

### Test 12: Container - FECProcess WithDb
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
./test-locally.sh --docker-profile withdb --job FECProcess --no-prompt
```

**Expected Output:**
- PostgreSQL container running
- FECProcess executes successfully
- Environment: Development
- Execution Mode: WithDb (PostgreSQL)
- Duration: ~10 seconds
- Exit Code: 0

---

## 🧪 Automated Test Scripts

### VM - Run All 6 Tests (PowerShell)

```powershell
# Navigate to worker directory
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker

Write-Host "`n=== Test 1: HealthCheck NoDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- HealthCheck

Write-Host "`n=== Test 2: HealthCheck WithDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- HealthCheck

Write-Host "`n=== Test 3: ScheduleJobs NoDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- ScheduleJobs

Write-Host "`n=== Test 4: ScheduleJobs WithDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- ScheduleJobs

Write-Host "`n=== Test 5: FECProcess NoDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Demo"
dotnet run -- FECProcess

Write-Host "`n=== Test 6: FECProcess WithDb ===" -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run -- FECProcess

Write-Host "`n=== All VM Tests Completed ===" -ForegroundColor Green
```

---

### Container - Run All 6 Tests (Bash)

```bash
#!/bin/bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs

echo "=== Test 7: Container - HealthCheck NoDb ==="
./test-locally.sh --docker-profile nodb --job HealthCheck --no-prompt || exit 1

echo "=== Test 8: Container - HealthCheck WithDb ==="
./test-locally.sh --docker-profile withdb --job HealthCheck --no-prompt || exit 1

echo "=== Test 9: Container - ScheduleJobs NoDb ==="
./test-locally.sh --docker-profile nodb --job ScheduleJobs --no-prompt || exit 1

echo "=== Test 10: Container - ScheduleJobs WithDb ==="
./test-locally.sh --docker-profile withdb --job ScheduleJobs --no-prompt || exit 1

echo "=== Test 11: Container - FECProcess NoDb ==="
./test-locally.sh --docker-profile nodb --job FECProcess --no-prompt || exit 1

echo "=== Test 12: Container - FECProcess WithDb ==="
./test-locally.sh --docker-profile withdb --job FECProcess --no-prompt || exit 1

echo "=== All Container Tests Completed ==="
```

---

## ✅ Success Criteria

### For Each Test

- [ ] **Exit Code**: 0 (success)
- [ ] **Status**: Completed
- [ ] **Logs**: Human-readable format
- [ ] **Environment**: Correct (Demo vs Development)
- [ ] **Execution Mode**: Correct (NoDb vs WithDb)
- [ ] **Duration**: Within expected range
- [ ] **Graceful Shutdown**: True

### NoDb Tests (Tests 1, 3, 5, 7, 9, 11)

- [ ] No database connection strings in logs
- [ ] No SQL commands executed
- [ ] In-memory repositories used
- [ ] Faster execution (~2-6 seconds)

### WithDb Tests (Tests 2, 4, 6, 8, 10, 12)

- [ ] Database connection successful
- [ ] SQL operations visible in logs
- [ ] Lock acquisition/release logged
- [ ] Execution records created
- [ ] Longer execution (~10-20 seconds)

---

## 🔍 Verification Commands

### VM - Check Database Records
```powershell
# Connect to PostgreSQL
psql -h localhost -U postgres -d batchjobs

# Check tables
\dt operational.*

# View job executions
SELECT * FROM operational.tbljobqueue ORDER BY created_at DESC LIMIT 10;

# View job master
SELECT * FROM operational.tbljobmaster;

# View locks (should be empty after successful run)
SELECT * FROM operational.batch_lock;
```

---

### Container - Check Database Records
```bash
# Connect to PostgreSQL container
docker exec -it batch_jobs_postgres psql -U postgres -d batchjobs

# Check tables
\dt operational.*

# View job executions
SELECT * FROM operational.tbljobqueue ORDER BY created_at DESC LIMIT 10;

# View job master
SELECT * FROM operational.tbljobmaster;

# View locks
SELECT * FROM operational.batch_lock;
```

---

## 🧹 Cleanup Commands

### VM Cleanup
```powershell
# Clear environment variable
Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue

# Optional: Clean database
psql -h localhost -U postgres -d batchjobs -c "TRUNCATE TABLE operational.tbljobqueue CASCADE;"
```

---

### Container Cleanup
```bash
# Stop all containers
docker-compose --profile withdb down
docker-compose --profile nodb down

# Remove volumes (deletes database data)
docker-compose down -v

# Remove built images (optional)
docker-compose build --no-cache
```

---

## 📊 Test Results Log Template

```
Test Execution Date: _______________
Tester: _______________
Branch: A-Foundation
Commit: _______________

VM Tests:
[ ] Test 1: HealthCheck NoDb - Duration: _____s - Exit Code: _____
[ ] Test 2: HealthCheck WithDb - Duration: _____s - Exit Code: _____
[ ] Test 3: ScheduleJobs NoDb - Duration: _____s - Exit Code: _____
[ ] Test 4: ScheduleJobs WithDb - Duration: _____s - Exit Code: _____
[ ] Test 5: FECProcess NoDb - Duration: _____s - Exit Code: _____
[ ] Test 6: FECProcess WithDb - Duration: _____s - Exit Code: _____

Container Tests (GitHub Codespaces):
[ ] Test 7: Container HealthCheck NoDb - Duration: _____s - Exit Code: _____
[ ] Test 8: Container HealthCheck WithDb - Duration: _____s - Exit Code: _____
[ ] Test 9: Container ScheduleJobs NoDb - Duration: _____s - Exit Code: _____
[ ] Test 10: Container ScheduleJobs WithDb - Duration: _____s - Exit Code: _____
[ ] Test 11: Container FECProcess NoDb - Duration: _____s - Exit Code: _____
[ ] Test 12: Container FECProcess WithDb - Duration: _____s - Exit Code: _____

Overall Status: [ ] PASS [ ] FAIL
Notes: _________________________________
```

---

## 🎯 Summary

| Environment | Tests | Status | Next Steps |
|-------------|-------|--------|------------|
| **VM** | 6/6 | ✅ Complete | Maintain regression cadence |
| **Container** | 6 scenarios defined | ✅ Ready to run | Execute and record results |

**Total Coverage: 12 scenarios defined (6 VM + 6 Container)**

---

## 📚 Related Documentation

- [VM Testing Guide](Apha.BatchJobs.Worker/VM-TESTING-GUIDE.md)
- [Container Testing Guide](CONTAINER-TESTING-GUIDE.md)
- [Testing Guide Index](TESTING-GUIDE-INDEX.md)
- [Security Best Practices](Apha.BatchJobs.Worker/SECURITY-BEST-PRACTICES.md)

---

**Last Updated:** 2026-04-23  
**Branch:** A-Foundation  
**Commit:** 8717697
