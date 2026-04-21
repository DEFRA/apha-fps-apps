# Quick Start Testing Scripts

Two one-command scripts to test the foundation layer locally. They now auto-select the best mode for the host:

- Linux Docker host: containerized validation with docker-compose
- Windows Server / Windows containers host: native .NET validation fallback

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
./test-locally.ps1                        # Auto-select docker or native mode
./test-locally.ps1 -NoPrompt              # Non-interactive mode
./test-locally.ps1 -Native                # Force native .NET mode
./test-locally.ps1 -DockerProfile withdb  # Docker: app + postgres
./test-locally.ps1 -DockerProfile nodb    # Docker: app only (NoDb)
./test-locally.ps1 -JobName HealthCheck   # Run a specific job
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
./src/Apha.BatchJobs/test-locally.sh         # Auto-select docker or native mode
./src/Apha.BatchJobs/test-locally.sh --no-prompt
./src/Apha.BatchJobs/test-locally.sh --native
./src/Apha.BatchJobs/test-locally.sh --docker-profile withdb
./src/Apha.BatchJobs/test-locally.sh --docker-profile nodb
./src/Apha.BatchJobs/test-locally.sh --job HealthCheck
./src/Apha.BatchJobs/test-locally.sh logs    # View logs from running containers
./src/Apha.BatchJobs/test-locally.sh stop    # Stop all containers
./src/Apha.BatchJobs/test-locally.sh clean   # Clean and restart fresh
```

## Validation Pass Matrix

1. Pass 1: NoDb local app
	- VS profile: BatchJobs NoDb FirstPass
	- CLI: dotnet run --project src/Apha.BatchJobs/BatchJobs.csproj --launch-profile "BatchJobs NoDb FirstPass"
2. Pass 2: WithDb local app + local postgres
	- Start postgres container only: docker-compose --profile withdb up -d postgres
	- VS profile: BatchJobs WithDb LocalPostgres
3. Pass 3: WithDb containerized app + postgres
	- PowerShell: ./test-locally.ps1 -DockerProfile withdb
	- Bash: ./test-locally.sh --docker-profile withdb
4. Pass 4: NoDb containerized app only
	- PowerShell: ./test-locally.ps1 -DockerProfile nodb
	- Bash: ./test-locally.sh --docker-profile nodb

## What Happens

1. Checks host/container mode
2. Uses Docker when Linux containers are available
3. Uses selected docker profile: withdb (app + postgres) or nodb (app only)
4. Falls back to native .NET when Docker is unavailable or in Windows container mode
5. Runs the HealthCheck job and prints logs
6. Leaves Docker-only commands available when docker mode is active

## Expected Output

```
Execution mode: native

========================================
Running Native .NET Validation
========================================

[INF] Batch Jobs Worker - Starting
[INF] Requested job: HealthCheck
[INF] === HealthCheck Job Started ===
[INF] Phase 1: Validating configuration...
...
[INF] === HealthCheck Job Completed Successfully ===
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
- On Linux container hosts, start Docker and rerun the script.
- On Windows Server hosts, the script will fall back to native mode automatically.

### Windows Server host
- This repo targets Linux containers for AWS ECS Fargate.
- On Windows Server, use the script's native fallback for local validation.
- Use a Linux-capable host or CI runner for container-image validation.

### Port already in use
```bash
# Check what's using 5432
lsof -i :5432

# Kill process or modify docker-compose.yml
```

## See Also
- [DEMO_RUNBOOK.md](DEMO_RUNBOOK.md) - Demo flow, required toggles, and expected logs by program stage
- [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md) - Comprehensive testing guide
- [ECR_DEPLOYMENT_GUIDE.md](ECR_DEPLOYMENT_GUIDE.md) - AWS deployment guide
