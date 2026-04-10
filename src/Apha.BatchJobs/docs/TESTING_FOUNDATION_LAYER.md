# Foundation Layer Testing Summary

## What You Can Test Locally

Your foundation layer is now fully testable **without any AWS resources or environment variables**. Everything runs locally with Docker.

---

## Quick Start (30 seconds)

**Windows (PowerShell):**
```powershell
cd src\Apha.BatchJobs
.\test-locally.ps1
```

**Mac/Linux (Bash):**
```bash
cd src/Apha.BatchJobs
./test-locally.sh
```

**Result:** PostgreSQL + Batch Job run, you watch the logs, container exits when done.

---

## What Gets Tested

### ✅ Logging Framework
- Serilog console logging with timestamps
- Log levels (Information, Warning, Error)
- Contextual properties in logs
- Job lifecycle phases logged

### ✅ Configuration Management
- appsettings.json configuration binding
- Environment variable overrides
- DatabaseSettings validation
- BatchJobSettings defaults

### ✅ Database Layer
- PostgreSQL connection string building
- DbContext functionality
- BatchLock table creation/queries
- JobExecutionRecord tracking
- Transaction handling

### ✅ Dependency Injection
- Service container setup
- Factory pattern implementation
- Repository registration
- Scope management

### ✅ Job Execution Framework
- Job factory resolution
- Command-line argument parsing
- Environment variable input (BATCH_JOB_NAME)
- Proper exception handling
- Exit codes (0=success, 1=fatal, 2=factory error, 3=cancelled)

### ✅ Docker & Containerization
- Multi-stage Docker build
- PostgreSQL container integration
- docker-compose orchestration
- Container lifecycle (startup → execution → exit)
- Non-root user execution

---

## Test Flows

### Test 1: Validate Basic Job Execution (30 seconds)

```bash
./test-locally.ps1
```

**Validates:**
- Docker image builds successfully
- PostgreSQL starts and initializes
- DI container wires up correctly
- HealthCheck job executes with 50 records
- All 4 phases complete (config validation, processing, DB check, reporting)
- Container exits with code 0

**Look for in logs:**
```
=== HealthCheck Job Started ===
Phase 1: Validating configuration...
Phase 2: Processing records...
  Processed 10/50 records
  Processed 20/50 records
  ...
Phase 4: Job completion report
=== HealthCheck Job Completed Successfully ===
```

### Test 2: View Logs Without Restarting

```bash
./test-locally.ps1 -LogsOnly   # (Windows)
./test-locally.sh logs         # (Mac/Linux)
```

**Validates:**
- Container logs persist
- You can view output after container exits
- Look for exit code in docker ps -a

### Test 3: Clean State Restart

```bash
./test-locally.ps1 -Clean      # (Windows)
./test-locally.sh clean        # (Mac/Linux)
```

**Validates:**
- Database is wiped and recreated fresh
- Container can run multiple times
- No leftover volumes or state

### Test 4: Stop Containers

```bash
./test-locally.ps1 -Stop       # (Windows)
./test-locally.sh stop         # (Mac/Linux)
```

**Validates:**
- Graceful shutdown
- PostgreSQL writes data before exit
- No orphaned containers

---

## Manual Command Tests (Advanced)

If scripts don't run, test manually:

### Command-Line Job Name

```bash
# Run locally with .NET CLI
cd src/Apha.BatchJobs
dotnet run -- HealthCheck
```

### Environment Variable Job Name

```bash
# Windows PowerShell
$env:BATCH_JOB_NAME="HealthCheck"
dotnet run

# Mac/Linux Bash
export BATCH_JOB_NAME="HealthCheck"
dotnet run
```

### Docker Run Directly

```bash
# Build
docker build -t batch-test .

# Run with manual env vars
docker run --rm \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DatabaseConnection__Server=host.docker.internal \
  -e DatabaseConnection__Port=5432 \
  -e DatabaseConnection__Database=batch_jobs_db \
  -e DatabaseConnection__Username=postgres \
  -e DatabaseConnection__Password=password \
  batch-test
```

---

## Expected Exit Codes

| Code | Meaning | Example |
|------|---------|---------|
| **0** | ✅ Success | Job completed, logs written to DB |
| **1** | ❌ Fatal Error | Unhandled exception, fatal crash |
| **2** | ⚠️ Factory Error | Job not registered or DI failure |
| **3** | ⏹️ Cancelled | CancellationToken triggered, task cancelled |

**Check exit code:**
```bash
# After container exits
docker ps -a
# Look for: "batch-jobs-app ... Exited (0)"
```

---

## Performance Baseline (Local Testing)

| Step | Time | Notes |
|------|------|-------|
| Docker image build | 15-30s | Only first run; cached after |
| PostgreSQL start | 5-10s | Waits for health check |
| .NET app startup | 2-3s | Resolves DI, loads config |
| HealthCheck execution | 3-5s | 50 records × 50ms delay |
| Container exit & cleanup | <1s | Graceful shutdown |
| **Total (cold)** | ~30-40s | First run with no cached layers |
| **Total (warm)** | ~10-15s | With built image & DB |

---

## Database Validation

After tests, verify database was used:

```bash
# Connect to PostgreSQL
docker exec -it batch_jobs_postgres psql -U postgres -d batch_jobs_db

# List tables
\dt

# Check operational schema
\dt+ operational.*

# View batch locks (should be empty after job)
SELECT * FROM operational.batch_lock;

# View execution records
SELECT * FROM operational.job_execution_record;
```

---

## Monitoring Container in Real-Time

While container is running, in another terminal:

```bash
# Watch container status
docker stats

# Monitor logs
docker logs -f batch-jobs-app

# Inspect container
docker inspect batch-jobs-app
```

---

## Failure Scenarios (Test Error Handling)

### Test Job Not Found
```bash
docker run --rm \
  --network host \
  -e BATCH_JOB_NAME="UnknownJob" \
  batch-test
```
**Expected:** Exit code 2, error message listing available jobs

### Test Config Missing
```bash
# Remove appsettings.json and try to run
```
**Expected:** Exit code 1, error message about config

### Test Database Unreachable
```bash
# Don't start PostgreSQL, run container
docker run --rm \
  -e DatabaseConnection__Server=localhost \
  batch-test
```
**Expected:** Exit code 1, timeout connecting to DB

---

## Documentation References

- **Quick start:** [TEST_SCRIPTS_README.md](TEST_SCRIPTS_README.md)
- **Detailed guide:** [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md)
- **ECR/AWS:** [ECR_DEPLOYMENT_GUIDE.md](ECR_DEPLOYMENT_GUIDE.md)
- **Architecture:** [BATCHJOBS_ARCHITECTURE_GUIDE.md](BATCHJOBS_ARCHITECTURE_GUIDE.md)

---

## What This Proves About Your Foundation

✅ **Configuration** - Loads from files + environment  
✅ **Dependency Injection** - Resolves services correctly  
✅ **Database** - Connects, creates tables, performs queries  
✅ **Logging** - Captures structured logs with context  
✅ **Containerization** - Builds, runs, and exits cleanly  
✅ **Error Handling** - Proper exception catching + exit codes  
✅ **Job Framework** - Factory resolves and executes jobs  
✅ **Docker Compose** - Multi-container orchestration works  

---

## Next Steps After Validation

1. ✅ Foundation verified locally
2. 🔄 Add more job handlers (same pattern as HealthCheck)
3. 🔄 Add integration tests (.xUnit or .NUnit)
4. 🔄 Test with real databases/data loads
5. 🔄 Setup CI/CD pipeline
6. 🔄 Deploy to AWS ECR and ECS

---

## Tips for Success

- **Always check exit codes** - They tell you what went wrong
- **Save logs before cleanup** - `docker logs > job.log` before `docker-compose down`
- **Test clean state regularly** - `./test-locally.ps1 -Clean` weekly
- **Use docker ps -a** - See all containers, even exited ones
- **Read the LOCAL_TESTING_GUIDE** - It has troubleshooting for common issues

