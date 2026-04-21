# Running Batch Jobs in Containers (GitHub Codespaces ONLY)

## ⚠️ IMPORTANT: Environment Requirements

**Docker is NOT available in the VM environment.**

- ✅ **VM Environment** → Use `.NET 10 SDK` directly (see `Apha.BatchJobs.Worker\VM-TESTING-GUIDE.md`)
- ✅ **GitHub Codespaces** → Use `docker-compose` (this guide)

**This guide is ONLY for GitHub Codespaces where Docker is available.**

---

## Two Simple Test Scenarios (GitHub Codespaces)

### Scenario 1: Container WITHOUT Database (NoDb Mode)

**Run:**
```bash
docker-compose --profile nodb up batch-jobs-nodb
```

**What happens:**
- ✅ Uses `ASPNETCORE_ENVIRONMENT=Demo`
- ✅ In-memory repositories (no PostgreSQL needed)
- ✅ Runs HealthCheck job
- ✅ Completes in ~4 seconds

---

### Scenario 2: Container WITH Database (WithDb Mode)

**Run:**
```bash
docker-compose --profile withdb up
```

**What happens:**
- ✅ Starts PostgreSQL container
- ✅ Initializes database with schema from `database/sql/*.sql`
- ✅ Uses `ASPNETCORE_ENVIRONMENT=Development`
- ✅ Connects to PostgreSQL
- ✅ Runs HealthCheck job
- ✅ Completes in ~13-17 seconds

---

## Configuration

### NoDb Mode (Demo)
```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Demo
  BATCH_JOB_NAME: HealthCheck
  BATCH_RUN_MODE: Manual
```

### WithDb Mode (Development)
```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Development
  ConnectionStrings__BatchJobsConnectionString: "Host=postgres;Port=5432;Database=batchjobs;Username=postgres;Password=postgres"
  BATCH_JOB_NAME: HealthCheck
  BATCH_RUN_MODE: Manual
```

---

## Database Initialization

PostgreSQL container automatically runs SQL scripts on first startup:
- `database/sql/001_batch_foundation_tables.sql`
- `database/sql/003_runtime_orchestrator_tables.sql`

These scripts are mounted as: `./database/sql:/docker-entrypoint-initdb.d:ro`

---

## Run Different Jobs

Change the `BATCH_JOB_NAME` environment variable:

```bash
# Run ScheduleJobs instead of HealthCheck
docker-compose --profile nodb run --rm -e BATCH_JOB_NAME=ScheduleJobs batch-jobs-nodb
```

---

## Cleanup

```bash
# Stop and remove containers
docker-compose --profile withdb down
docker-compose --profile nodb down

# Remove volumes (deletes database data)
docker-compose down -v
```

---

## GitHub Codespaces Setup

### Prerequisites:
- ✅ GitHub Codespaces environment (Docker is pre-installed)
- ✅ This repository cloned in Codespaces

### Steps:

1. **Open repository in GitHub Codespaces**
   - Go to GitHub repository
   - Click "Code" → "Codespaces" → "Create codespace on A-Foundation"

2. **Navigate to BatchJobs directory**
   ```bash
   cd src/Apha.BatchJobs
   ```

3. **Run the scenarios** (see commands above)

**Everything just works in Codespaces! ✅**

---

## ❌ NOT for Local VM

**Do NOT try to run these Docker commands in the VM environment:**
- VM does not have Docker installed
- VM does not support container workloads
- For VM testing, use `VM-TESTING-GUIDE.md` instead

---

## Troubleshooting

**Check logs:**
```bash
docker-compose --profile withdb logs -f
```

**Verify PostgreSQL is healthy:**
```bash
docker-compose --profile withdb ps
```

**Connect to PostgreSQL:**
```bash
docker exec -it batch_jobs_postgres psql -U postgres -d batchjobs
```

**Rebuild containers after code changes:**
```bash
docker-compose --profile withdb build
docker-compose --profile withdb up
```

---

## Summary

| Scenario | Command | Environment | Database | Duration |
|----------|---------|-------------|----------|----------|
| Container NoDb | `docker-compose --profile nodb up batch-jobs-nodb` | `Demo` | In-Memory | ~4s |
| Container WithDb | `docker-compose --profile withdb up` | `Development` | PostgreSQL | ~17s |

**That's it! No overcomplications, just config changes.** 🎉
