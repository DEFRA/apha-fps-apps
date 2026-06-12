# How to Run and Debug Batch Jobs (RecreateSummaries, MABArchive, HealthCheck)

## Architecture Overview

The batch jobs solution has **three job types**:

1. **HealthCheck** — Manual health validation job (ad-hoc)
2. **RecreateSummaries** — Manual/Scheduled job (rebuild summary tables)
3. **MABArchive** — Scheduled job (loads FPS data to MABArchive schema)

Jobs run via:
- **.NET Worker Host** (production background processor)
- **.NET API** (status queries, manual triggers)
- **Docker Compose** (local testing)
- **Local debugging** (Visual Studio)

---

## Quick Start: Run Jobs Locally

### Option 1: Docker Compose (Fastest)

```powershell
cd src\Apha.BatchJobs

# Start with database
docker-compose --profile withdb up --build

# In another terminal, trigger a job via API
curl -X GET "http://localhost:5000/api/batch-jobs"
```

**Profiles:**
- `withdb` — App + PostgreSQL
- `nodb` — App only (in-memory)

### Option 2: Local Testing Script (Windows)

```powershell
cd src\Apha.BatchJobs

# Default: runs HealthCheck
./test-locally.ps1

# Run specific job
./test-locally.ps1 -JobName "RecreateSummaries"

# No interactive prompts
./test-locally.ps1 -JobName "MABArchive" -NoPrompt
```

**Available Jobs:**
- `HealthCheck` (default)
- `RecreateSummaries`
- `MABArchive`
- `FECProcess`

### Option 3: .NET CLI (Command Line)

```powershell
cd src\Apha.BatchJobs

# Build
dotnet build BatchJobs.sln

# Run Worker with specific job
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:BATCH_JOB_NAME = "HealthCheck"
$env:PGPASSWORD = "LOCAL_DB_PASSWORD"
$env:ConnectionStrings__FPSConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=LOCAL_DB_PASSWORD;Database=batch_jobs_foundation_db"

dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

---

## Debug in Visual Studio

### Setup

1. **Open Solution**
   ```
   src/Apha.BatchJobs/BatchJobs.sln
   ```

2. **Set Startup Project**
   - Right-click Solution → Set Startup Projects
   - Select `Apha.BatchJobs.Worker` (or `Apha.BatchJobs.Api`)

3. **Configure Debug Profile**
   - Go to **Debug** → **Apha.BatchJobs.Worker Debug Properties**
   - Set Environment Variables:
     ```
     ASPNETCORE_ENVIRONMENT=Development
     BATCH_JOB_NAME=HealthCheck
     ConnectionStrings__FPSConnectionString=Host=localhost;Port=5432;Username=postgres;Password=LOCAL_DB_PASSWORD;Database=batch_jobs_foundation_db
     PGPASSWORD=LOCAL_DB_PASSWORD
     ```

4. **Start Database**
   ```powershell
   # If not already running
   docker-compose --profile withdb up -d postgres
   # or
   # psql -U postgres -h localhost
   ```

5. **Set Breakpoints** in:
   - `HealthCheckJobHandler.cs`
   - `RecreateSummariesJobHandler.cs`
   - `MabArchiveJobHandler.cs`

6. **Press F5** to debug

---

## Job-Specific Debug Workflows

### 1. Debug HealthCheck

**Purpose:** Validate logging, database connectivity, and execution tracking

**Entry Point:**  
`Apha.BatchJobs.Application/Jobs/ManualJobs/HealthCheck/HealthCheck/HealthCheckJobHandler.cs`

**Debug Steps:**

```csharp
// In HealthCheckJobHandler.ExecuteAsync()
_logger.LogInformation("Health check validating database...");

// Will log:
// - Database connectivity
// - Logging framework status
// - Execution tracking
```

**Run via CLI:**
```powershell
$env:BATCH_JOB_NAME = "HealthCheck"
dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

**Expected Output:**
```
Health check job started
[✓] Database connection OK
[✓] Logging framework operational
[✓] Execution tracking verified
Health check job completed successfully
```

---

### 2. Debug RecreateSummaries

**Purpose:** Rebuild summary tables from base data (manually triggered)

**Entry Points:**  
- Manual: `Apha.BatchJobs.Application/Jobs/ManualJobs/RecreateSummaries/RecreateSummaries/RecreateSummariesHandler.cs`
- Scheduled: `Apha.BatchJobs.Application/Jobs/ScheduledJobs/RecreateSummaries/RecreateSummariesJobHandler.cs`

**Debug Steps:**

```csharp
// Manual trigger (from UI)
public async Task ExecuteAsync(CancellationToken cancellationToken = default)
{
    var results = await _orchestrator.ExecuteAsync(
        runId,
        month: 1,           // FPS period month
        triggeredBy: "user@example.com",
        cancellationToken);
    
    // Results contain per-step execution tracking
}
```

**Execution Steps Performed:**
1. Delete existing FPS totals
2. Create FPS totals from views
3. Insert missing projects
4. Delete/create time cost calculations
5. Delete/create project month variants
6. ... (14 steps total)

**Run via CLI:**
```powershell
$env:BATCH_JOB_NAME = "RecreateSummaries"
$env:BATCH_JOB_CONTEXT = '{"Month":1,"TriggeredBy":"test-user"}'
dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

**Database Tables Modified:**
- `fps.fpsyeartotals` (summary totals)
- `fps.projectmonth*` (working tables)
- `fps.recreatesummaries_log` (execution log)

---

### 3. Debug MABArchive

**Purpose:** Load FPS data into MABArchive schema (scheduled weekly)

**Entry Point:**  
`Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/MabArchiveJobHandler.cs`

**Schedule:**  
- **Runs:** Monday-Friday, 8:00 PM UTC
- **Idempotency:** Year-scoped, deterministic ordering

**Debug Steps:**

```csharp
// Load current + previous year data
var results = await _orchestrator.ExecuteAsync(
    runId,
    executionYear: 2026,     // Current/previous calendar year
    cancellationToken);

// Writes to mabarchive schema:
// - my_fpsyeartotals (yearly snapshots)
// - my_tlkpproject_all (project state)
```

**Run via CLI:**
```powershell
$env:BATCH_JOB_NAME = "MABArchive"
$env:BATCH_JOB_CONTEXT = '{"ExecutionYear":2026}'
dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

**Database Tables Modified:**
- `mabarchive.my_fpsyeartotals` (yearly aggregates)
- `mabarchive.my_tlkpproject_all` (project snapshots)

---

## API-Based Triggering

The `Apha.BatchJobs.Api` provides REST endpoints for:
- Checking job status
- Triggering manual jobs
- Querying execution history

### Start API Server

```powershell
cd src\Apha.BatchJobs

dotnet run --project Apha.BatchJobs.Api/Apha.BatchJobs.Api.csproj
```

**Server starts on:** `http://localhost:5000`

### API Endpoints

**1. Get All Job Statuses**
```bash
GET http://localhost:5000/api/batch-jobs
```

Response:
```json
[
  {
    "jobName": "HealthCheck",
    "isRunning": false,
    "lastExecutedAt": "2026-05-12T11:45:00Z",
    "lastStatus": "Success"
  },
  {
    "jobName": "RecreateSummaries",
    "isRunning": false,
    "lastExecutedAt": "2026-05-12T10:30:00Z",
    "lastStatus": "Success"
  }
]
```

**2. Get Specific Job Status**
```bash
GET http://localhost:5000/api/batch-jobs/HealthCheck/status
```

**3. Trigger Manual Job** (via ECS Task Dispatcher)
```bash
POST http://localhost:5000/api/batch-jobs/RecreateSummaries/trigger
Content-Type: application/json

{
  "month": 1,
  "triggeredBy": "developer@example.com"
}
```

---

## Troubleshooting

### Job Won't Start

**Check logs:**
```powershell
# Docker
docker-compose logs api
docker-compose logs worker

# Local
# Check Visual Studio output window
```

**Common Issues:**

| Issue | Solution |
|-------|----------|
| `ConnectionRefused` | Start PostgreSQL: `docker-compose up -d postgres` |
| `BATCH_JOB_NAME not set` | Set environment variable before running |
| `Database permission denied` | Verify connection string and credentials |
| `Job already running` | Wait for previous execution to complete (check status API) |

### Database Connection Issues

```powershell
# Test connection
$env:PGPASSWORD = "LOCAL_DB_PASSWORD"
psql -h localhost -U postgres -d batch_jobs_foundation_db -c "SELECT version();"

# Check if seeded data exists
psql -h localhost -U postgres -d batch_jobs_foundation_db -c "SELECT COUNT(*) FROM fps.tlkpproject;"
```

### Debugging SQL Queries

Enable SQL logging in **appsettings.Development.json**:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## Project Structure Reference

```
src/Apha.BatchJobs/
├── Apha.BatchJobs.Api/                 ← REST API for status/triggering
│   ├── Controllers/
│   │   └── JobStatusController.cs       ← Status & trigger endpoints
│   ├── Services/
│   │   ├── IJobStatusService.cs
│   │   └── IEcsTaskDispatcher.cs        ← ECS task invocation
│   └── Program.cs                       ← API startup configuration
│
├── Apha.BatchJobs.Worker/              ← Background job host
│   ├── Services/
│   │   ├── JobOrchestrator.cs           ← Executes job steps
│   │   └── JobLockService.cs            ← Distributed lock management
│   └── Program.cs                       ← Worker startup configuration
│
├── Apha.BatchJobs.Application/          ← Business logic
│   ├── Jobs/
│   │   ├── ManualJobs/
│   │   │   ├── HealthCheck/             ← Validation job
│   │   │   ├── RecreateSummaries/       ← Summary rebuild (manual)
│   │   │   └── FECProcess/              ← FEC processing
│   │   └── ScheduledJobs/
│   │       ├── RecreateSummaries/       ← Summary rebuild (scheduled)
│   │       └── MABArchive/              ← Archive load job
│   ├── Interfaces/
│   │   └── IBatchJob.cs                 ← Job contract
│   └── DependencyInjection/
│       └── ServiceCollectionSetup.cs    ← DI registration
│
├── Apha.BatchJobs.Infrastructure/       ← Database & persistence
│   ├── Repositories/
│   ├── Sql/
│   │   └── RecreateSummaries/           ← SQL steps (01-17)
│   └── Context/
│       ├── BatchJobsDbContext.cs        ← Entity Framework context
│       └── RecreateSummariesContext.cs
│
├── Apha.BatchJobs.Domain/               ← Interfaces & entities
├── Apha.BatchJobs.UnitTests/            ← Unit & integration tests
│
└── docs/database/                       ← Database assets
    ├── sql/
    │   ├── reseed-local-db.ps1          ← Reset localhost data
    │   ├── validation-sp-execution.sql  ← SP validation test
    │   └── README.md                    ← SQL operations guide
    └── validation/
        └── VALIDATION-REPORT.md         ← Latest test results
```

---

## Testing Checklist

When debugging a new feature in a job:

- [ ] **Unit Test** — Add test in `Apha.BatchJobs.UnitTests/`
- [ ] **Local Run** — Execute via `test-locally.ps1`
- [ ] **Debug Session** — Step through in Visual Studio
- [ ] **Database Validation** — Query affected tables post-execution
- [ ] **API Test** — Call status endpoints
- [ ] **Docker Test** — Run via `docker-compose --profile withdb up`
- [ ] **Integration Test** — Verify cross-job dependencies

---

## Performance Profiling

For long-running jobs (RecreateSummaries, MABArchive):

**1. Enable Query Timing:**
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Connection": "Debug"
    }
  }
}
```

**2. Check SQL Execution Time:**
```sql
-- Query recreatesummaries_log for execution times
SELECT userid, period, datedone, 
       EXTRACT(EPOCH FROM (datedone - lag(datedone) OVER (ORDER BY datedone))) as duration_seconds
FROM fps.recreatesummaries_log
ORDER BY datedone DESC LIMIT 10;
```

**3. Profile in Visual Studio:**
- Debug → Performance Profiler
- Record CPU usage and memory allocation
- Identify slow steps

---

## Next Steps

1. **Start with HealthCheck** — Simplest job, validates basic infrastructure
2. **Move to RecreateSummaries** — More complex, tests SQL orchestration
3. **Advanced: MABArchive** — Scheduled job with year-scoped logic
4. **Production:** Use Docker/ECS for deployment

Good luck with your batch job development! 🚀
