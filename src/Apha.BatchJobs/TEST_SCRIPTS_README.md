# Quick Start Testing Scripts

Two one-command scripts to test the foundation layer locally:

## PowerShell (Windows)

```powershell
# From repo root (recommended)
./test-locally.ps1

# Or from BatchJobs folder
cd src/Apha.BatchJobs
./test-locally.ps1
```

### Commands:
```powershell
./test-locally.ps1                        # Build, start services, run job
./test-locally.ps1 -LogsOnly              # View logs from running containers
./test-locally.ps1 -Stop                  # Stop all containers
./test-locally.ps1 -Clean                 # Clean and restart fresh
./src/Apha.BatchJobs/test-locally.ps1     # Direct script path from repo root
```

## Bash (macOS / Linux)

```bash
# From repo root
chmod +x src/Apha.BatchJobs/test-locally.sh
./src/Apha.BatchJobs/test-locally.sh

# Or from BatchJobs folder
cd src/Apha.BatchJobs
chmod +x test-locally.sh
./test-locally.sh
```

### Commands:
```bash
./src/Apha.BatchJobs/test-locally.sh         # Build, start services, run job
./src/Apha.BatchJobs/test-locally.sh logs    # View logs from running containers
./src/Apha.BatchJobs/test-locally.sh stop    # Stop all containers
./src/Apha.BatchJobs/test-locally.sh clean   # Clean and restart fresh
```

## What Happens

1. ✓ Checks Docker is running
2. ✓ Builds BatchJobs Docker image
3. ✓ Starts PostgreSQL container
4. ✓ Starts batch job container
5. ✓ Streams live logs
6. ✓ Container exits when job completes
7. ✓ Shows exit code (0 = success)

## Expected Output

```
========================================
Starting Services with docker-compose
========================================

Starting PostgreSQL and Batch Job...
[+] Building...
batch-jobs | ===========================================
batch-jobs | Batch Jobs Worker - Starting
batch-jobs | ===========================================
batch-jobs | Timestamp: 2026-04-10 12:30:45.123
batch-jobs | ProcessId: 1234
batch-jobs | Available jobs: HealthCheck
batch-jobs | 
batch-jobs | === HealthCheck Job Started ===
batch-jobs | Phase 1: Validating configuration...
...
batch-jobs | === HealthCheck Job Completed Successfully ===
batch-jobs exited with code 0
```

## Troubleshooting

### Script won't run

**PowerShell:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
./test-locally.ps1
```

**Bash:**
```bash
chmod +x test-locally.sh
./test-locally.sh
```

### Docker not running
- Start Docker Desktop
- Wait for "Docker Engine is running"
- Run script again

### Port already in use
```bash
# Check what's using 5432
lsof -i :5432

# Kill process or modify docker-compose.yml
```

## See Also
- [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md) - Comprehensive testing guide
- [ECR_DEPLOYMENT_GUIDE.md](ECR_DEPLOYMENT_GUIDE.md) - AWS deployment guide
