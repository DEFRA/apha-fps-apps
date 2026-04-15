# Local Testing Guide - Batch Jobs Foundation Layer

## Overview

This guide walks you through testing the BatchJobs foundation layer locally. You'll be able to:
- Build a Docker image locally
- Spin up PostgreSQL in a container
- Run the batch job and watch it execute
- See the container exit when complete
- View all logs

The recommended mode depends on the host:
- Linux-capable Docker host: containerized validation with `docker-compose`
- Windows Server host: native `.NET` validation via the local test script fallback

## Prerequisites

✅ **Required:**
- Docker Desktop installed and running
- .NET 10.0 SDK (for local builds)
- PowerShell or bash terminal

✅ **Optional (for local .NET testing without Docker):**
- PostgreSQL 16 running locally

---

## Quick Start (Fastest Way)

### 1. Build and Run Everything with the Local Test Script

Recommended command:

```powershell
cd src/Apha.BatchJobs
./test-locally.ps1 -NoPrompt
```

On Linux/macOS:

```bash
cd src/Apha.BatchJobs
./test-locally.sh --no-prompt
```

What happens:
- On Linux Docker hosts, the script uses `docker-compose`
- On Windows Server / Windows container hosts, the script falls back to native `.NET`

### 2. Build and Run Everything with Docker Compose

The easiest way - one command spins up PostgreSQL + runs the batch job:

```bash
cd src/Apha.BatchJobs
docker-compose up --build
```

**What happens:**
1. PostgreSQL starts and initializes
2. Application builds and runs
3. HealthCheck job executes
4. Container outputs logs
5. Container exits automatically when done
6. PostgreSQL continues running

**Expected output (last 10 lines):**
```
batch-jobs | ===========================================
batch-jobs | Batch Jobs Worker - Starting
batch-jobs | ===========================================
batch-jobs | Timestamp: 2026-04-10 12:30:45.123
batch-jobs | ProcessId: 1234
batch-jobs | Total services registered: 12
batch-jobs | Requested job: HealthCheck
batch-jobs | Available jobs: HealthCheck
batch-jobs | Creating job handler for 'HealthCheck'...
batch-jobs | === HealthCheck Job Started ===
```

**To stop containers:**
```bash
docker-compose down
```

---

## Intermediate Testing (Step by Step)

### Step 1: Build Image Locally

```bash
cd src/Apha.BatchJobs
docker build -t apha-batch-jobs:dev .
```

**Verify build succeeded:**
```bash
docker images | grep apha-batch-jobs
```

Expected output:
```
REPOSITORY          TAG       IMAGE ID      CREATED      SIZE
apha-batch-jobs     dev       a1b2c3d4e5    1 min ago    186MB
```

### Step 2: Start PostgreSQL Container (Manual)

```bash
docker run -d \
  --name batch_jobs_postgres \
  -e POSTGRES_DB=batch_jobs_foundation_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=password \
  -p 5432:5432 \
  postgres:16-alpine
```

**Wait for it to be ready:**
```bash
docker logs -f batch_jobs_postgres
```

Look for: `database system is ready to accept connections`

### Step 3: Run Batch Job Container

#### Option A: Using `docker run` command

```bash
docker run --rm \
  --name batch-jobs-app \
  --network host \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DatabaseConnection__Server=localhost \
  -e DatabaseConnection__Port=5432 \
  -e DatabaseConnection__Database=batch_jobs_foundation_db \
  -e DatabaseConnection__Username=postgres \
  -e DatabaseConnection__Password=<REDACTED> \
  apha-batch-jobs:dev
```

#### Option B: Using docker-compose (Recommended)

```bash
# Start PostgreSQL
docker-compose up -d postgres

# Run the batch job
docker-compose up batch-jobs
```

### Step 4: View Logs in Real-Time

**While container is running:**
```bash
docker logs -f batch-jobs-app
```

**After container exits:**
```bash
docker logs batch-jobs-app
```

### Step 5: Verify Container Exited

```bash
docker ps -a | grep batch-jobs-app
```

Look for `Exited (0)` - exit code 0 = success

Exit codes:
- `0` - Successfully completed
- `1` - Fatal/unhandled error
- `2` - Job not found or factory error
- `3` - Job was cancelled

---

## Advanced Testing (Local Without Docker)

This is the recommended local mode on Windows Server hosts where Docker is running in Windows container mode.

### Build and Run Locally

```bash
cd src/Apha.BatchJobs

# Build
dotnet build

# Run (with PostgreSQL running on localhost)
dotnet run -- HealthCheck
```

### Environment Variables (Local)

Set for the terminal session:

**PowerShell:**
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DatabaseConnection__Server="localhost"
$env:DatabaseConnection__Port="5432"
$env:DatabaseConnection__Database="batch_jobs_foundation_db"
$env:DatabaseConnection__Username="postgres"
$env:DatabaseConnection__Password="password"
$env:BATCH_JOB_NAME="HealthCheck"

dotnet run
```

**Bash:**
```bash
export ASPNETCORE_ENVIRONMENT=Development
export DatabaseConnection__Server=localhost
export DatabaseConnection__Port=5432
export DatabaseConnection__Database=batch_jobs_foundation_db
export DatabaseConnection__Username=postgres
export DatabaseConnection__Password=<REDACTED>
export BATCH_JOB_NAME=HealthCheck

dotnet run
```

---

## Testing Different Job Execution Modes

### Execute HealthCheck Job Explicitly

```bash
# Via command-line argument
dotnet run -- HealthCheck

# Via container
docker compose run --rm batch-jobs HealthCheck
```

### List Available Jobs

The application logs available jobs automatically:
```
Available jobs: HealthCheck
```

To extend with more jobs later:
1. Create new job handler in `Apha.BatchJobs.Application/Jobs/{JobName}/`
2. Register in `DependencyInjection.cs` jobRegistry
3. Run again

---

## Docker Compose Commands Reference

```bash
# Start all services
docker-compose up

# Start in background
docker-compose up -d

# Rebuild and start
docker-compose up --build

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f batch-jobs
docker-compose logs -f postgres

# Stop all services
docker-compose stop

# Stop and remove all services and volumes
docker-compose down

# Remove all images
docker-compose down --rmi all

# Run one-off command in container
docker-compose run --rm batch-jobs HealthCheck

# Execute command in running container
docker-compose exec batch-jobs sh
```

---

## Troubleshooting

### "Connection refused" / "PostgreSQL not running"

**Solution 1: Check if PostgreSQL is ready**
```bash
docker ps
docker logs batch_jobs_postgres
```

**Solution 2: Wait and retry**
```bash
# docker-compose has a healthcheck, wait 10-15 seconds
docker-compose logs postgres
# Should see "database system is ready"
```

**Solution 3: Remove and restart**
```bash
docker-compose down
docker-compose up --build
```

### "Container exits immediately"

Check logs:
```bash
docker logs batch-jobs-app
```

Look for error messages like:
- `DatabaseConnection configuration is missing` - Check appsettings.json
- `Job 'JobName' is not registered` - Check job name in registry
- `Connection timeout` - PostgreSQL isn't ready (wait 5-10 seconds)

### Docker Desktop not running

**MacOS/Windows:**
- Open Docker Desktop application
- Wait for "Docker Engine is running" message

### Port 5432 already in use

**Find what's using it:**
```bash
# Windows
netstat -ano | findstr :5432

# Mac/Linux
lsof -i :5432
```

**Kill the process or use different port:**
```yaml
# In docker-compose.yml, change:
#   ports:
#     - "5433:5432"  # Use 5433 instead
```

---

## What to Look For (Success Indicators)

### ✅ Successful Job Execution

```
===========================================
Batch Jobs Worker - Starting
========================================== 
Timestamp: 2026-04-10 12:30:45.123
ProcessId: 1234
Total services registered: 12

=== HealthCheck Job Started ===
Job: HealthCheck
Phase 1: Validating configuration...
  Environment: Development
  .NET Version: .NET 10.0
  OS: Linux ...

Phase 2: Processing records...
  Processed 10/50 records
  Processed 20/50 records
  ...
Phase 2 completed

===========================================
Batch job 'HealthCheck' completed successfully
===========================================
```

**Exit code should be: `0`**

### ❌ Failed Execution

```
ERROR: Job 'UnknownJob' is not registered. Available jobs: HealthCheck

Exit code: 2
```

**Exit code: `2` (factory error)**

---

## Performance Baseline (Local)

Expected timings on modern machine:

| Operation | Duration |
|-----------|----------|
| Docker image build | 15-30 seconds |
| PostgreSQL start | 5-10 seconds |
| .NET app launch | 2-3 seconds |
| HealthCheck job execution | 3-5 seconds |
| Total (cold start) | ~30-40 seconds |
| Total (warm - containers cached) | ~10-15 seconds |

---

## Next Steps After Successful Local Testing

1. **Run more jobs** - Add new job handlers in `Application/Jobs/`
2. **Test database integration** - Create execution records in DB
3. **Load testing** - Run multiple tasks via docker-compose
4. **CI/CD validation** - Commit changes and push
5. **Cloud deployment** - Build and push to ECR when ready

---

## Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [.NET Runtime Docker Image](https://hub.docker.com/_/microsoft-dotnet/runtime/)
- [Serilog Console Sink](https://github.com/serilog/serilog-sinks-console)

