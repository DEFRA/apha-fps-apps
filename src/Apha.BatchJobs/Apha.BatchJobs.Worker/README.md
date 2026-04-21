# Batch Jobs Worker - Quick Start Guide

This guide explains how to run batch jobs in a VM environment with and without a database.

## Prerequisites

- **.NET 10.0 SDK** installed
- **PostgreSQL 16** (only required for WithDb mode)
- Visual Studio 2026 or VS Code (optional)

## Test Scenarios

### Test 1: Run Without Database (Demo Mode - NoDb)

This mode uses in-memory repositories and **does not require PostgreSQL**.

#### Steps:

1. Navigate to the Worker directory:
   ```powershell
   cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
   ```

2. Set environment to **Demo**:
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Demo"
   ```

3. Run the batch job:
   ```powershell
   dotnet run -- HealthCheck
   ```

#### Expected Output:
```
✓ Status: Completed
✓ Exit Code: 0
✓ Duration: ~4 seconds
✓ Database: In-Memory (NoDb)
✓ Records Processed: 50/50
```

#### What Happens:
- Uses `NoDbBatchLockRepository` (in-memory lock management)
- Uses `NoDbJobExecutionRepository` (in-memory execution tracking)
- No database connection required
- Perfect for testing and demos

---

### Test 2: Run With Database (Development Mode - WithDb)

This mode uses **PostgreSQL database** for persistence.

#### Prerequisites:

1. **PostgreSQL must be running**:
   ```powershell
   Get-Service -Name "*postgres*"
   ```
   Expected: `Status: Running`

2. **Database must exist** with schema:
   - Database name: `batchjobs`
   - Schema: `operational`
   - Tables: `batch_lock`, `tbljobmaster`, `tbljobstatus`, `tbljobqueue`, `tbljobqueue_log`

3. **Connection string** configured in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "BatchJobsConnectionString": "Host=localhost;Port=5432;Database=batchjobs;Username=postgres;Password=postgres"
     }
   }
   ```

#### Steps:

1. Navigate to the Worker directory:
   ```powershell
   cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
   ```

2. Set environment to **Development**:
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   ```

3. Run the batch job:
   ```powershell
   dotnet run -- HealthCheck
   ```

#### Expected Output:
```
✓ Status: Completed
✓ Exit Code: 0
✓ Duration: ~13 seconds
✓ Database: PostgreSQL (localhost:5432)
✓ Records Processed: 50/50
✓ Lock acquired and released in database
✓ Execution record written to operational.tbljobqueue
```

#### What Happens:
- Connects to PostgreSQL database
- Acquires distributed lock in `operational.batch_lock` table
- Creates execution record in `operational.tbljobqueue`
- Logs execution history in `operational.tbljobqueue_log`
- Releases lock after completion
- Full persistence and audit trail

---

## Database Setup (For WithDb Mode)

If the database doesn't exist yet, run these SQL scripts in order:

1. **Create database**:
   ```sql
   CREATE DATABASE batchjobs;
   ```

2. **Run schema scripts** (from `database/sql` directory):
   - `001_batch_foundation_tables.sql` - Creates foundational tables
   - `003_runtime_orchestrator_tables.sql` - Creates lock table

---

## Interactive Demo Script

Use the provided demo script for an interactive experience:

```powershell
cd D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation\src\Apha.BatchJobs\Apha.BatchJobs.Worker
.\run-demo.ps1
```

**Menu Options:**
1. **Demo Mode (NoDb)** - Runs without PostgreSQL
2. **Production Mode (WithDb)** - Requires PostgreSQL connection
3. **Development Mode (WithDb)** - Local PostgreSQL

---

## Configuration Files

### appsettings.Demo.json (NoDb Mode)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
**Note**: No connection string needed!

### appsettings.Development.json (WithDb Mode)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=localhost;Port=5432;Database=batchjobs;Username=postgres;Password=postgres"
  }
}
```

### appsettings.Production.json (WithDb Mode)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=your-production-db-host;Port=5432;Database=batchjobs;Username=batchjobuser;Password=your-production-password"
  }
}
```

---

## Job Names

Available batch jobs:
- `HealthCheck` - System health check job (default)
- `ScheduleJobs` - Job scheduling
- `FECProcess` - FEC processing
- `RecreateSummaries` - Summary recreation

**Example**: Run different job
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run -- ScheduleJobs
```

---

## Environment Variables

| Variable | Values | Description |
|----------|--------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Demo`, `Development`, `Production` | Determines DB mode |
| `BATCH_JOB_NAME` | Job name | Alternative to command line arg |
| `BATCH_RUN_MODE` | `Manual`, `Scheduled` | Run mode for the job |

---

## Troubleshooting

### Issue: "Connection refused" or database connection errors

**Solution**: Verify PostgreSQL is running
```powershell
Get-Service -Name "*postgres*"
```

### Issue: "Database 'batchjobs' does not exist"

**Solution**: Create the database and run schema scripts
```powershell
# Run from database/sql directory
psql -U postgres -c "CREATE DATABASE batchjobs;"
psql -U postgres -d batchjobs -f 001_batch_foundation_tables.sql
psql -U postgres -d batchjobs -f 003_runtime_orchestrator_tables.sql
```

### Issue: "Method not found: ExecuteDeleteAsync"

**Solution**: Version mismatch - ensure all projects use same EF Core version (10.0.0)

### Issue: Security vulnerability warnings

**Solution**: All packages should reference `System.Security.Cryptography.Xml` version `9.0.15` or higher

---

## Architecture

### Demo Mode (NoDb)
```
Worker → NoDbBatchLockRepository (in-memory)
      → NoDbJobExecutionRepository (in-memory)
      → Job Handler (executes business logic)
```

### Development/Production Mode (WithDb)
```
Worker → BatchLockRepository (PostgreSQL)
      → JobExecutionRepository (PostgreSQL)
      → Job Handler (executes business logic)
      → BatchJobsDbContext (EF Core)
```

---

## Quick Reference

| Mode | Environment | Database Required | Use Case |
|------|-------------|-------------------|----------|
| NoDb | `Demo` | ❌ No | Testing, demos, development without DB |
| WithDb | `Development` | ✅ Yes (localhost) | Local development with persistence |
| WithDb | `Production` | ✅ Yes (remote) | Production deployment |

---

## Success Criteria

Both tests are successful when:

✅ Exit code = 0  
✅ Status = Completed  
✅ No exceptions in logs  
✅ Records processed = 50/50  
✅ Success rate = 100%  
✅ Graceful shutdown completed  

**Demo Mode**: Runs in ~4 seconds  
**WithDb Mode**: Runs in ~13 seconds (includes DB I/O)

---

## Next Steps

1. ✅ Run Test 1 (NoDb) - Verify application works without dependencies
2. ✅ Run Test 2 (WithDb) - Verify database integration works
3. Create custom batch jobs by implementing `IBatchJob` interface
4. Deploy to VM environment with appropriate `appsettings.{Environment}.json`
5. Configure monitoring and alerting for production

---

## Support

For issues or questions:
- Check logs in `Logs/BatchJobs.log` (Demo mode only)
- Review structured JSON logs in console output
- Verify PostgreSQL connectivity: `Test-NetConnection -ComputerName localhost -Port 5432`
- Check database tables exist: Connect to `batchjobs` database and verify `operational` schema

---

**Last Updated**: 2026-04-21  
**Version**: .NET 10.0  
**EF Core**: 10.0.0  
**PostgreSQL**: 16+
