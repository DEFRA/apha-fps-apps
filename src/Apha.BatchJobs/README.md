# Apha.BatchJobs

Short reference for running, testing, and operating the Batch Jobs foundation.

## 📋 Master Implementation Guide (START HERE!)

**Consolidated single document for team briefings:**

- **[MASTER-IMPLEMENTATION-GUIDE.md](docs/MASTER-IMPLEMENTATION-GUIDE.md)** ⭐ **READ THIS FIRST**
  - Complete architecture overview
  - 7-state machine specification with design rationale
  - Both API endpoints with full request/response examples
  - Watchdog algorithm with pseudocode
  - Design decision explanations for architects (Q&A format)
  - Deployment guide (local → staging → production)
  - Team responsibilities matrix
  - Implementation checklist
  - Sign-off section
  - **One document, everything you need**

## 📚 Detailed Reference Documents (As Needed)

**For specific deep-dives:**

- **[BATCHJOBS-STATE-MACHINE-CONTRACT.md](docs/BATCHJOBS-STATE-MACHINE-CONTRACT.md)** – Formal contract (sign-off required)
- **[PACT-API-OPENAPI.yaml](docs/PACT-API-OPENAPI.yaml)** – OpenAPI 3.0 spec (import to Postman/IDE)
- **[DEMO-END-TO-END-FLOW.md](docs/DEMO-END-TO-END-FLOW.md)** – Demo scenarios with timelines
- **[PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](docs/PACT-BATCHJOBS-HANDOFF-EVENTGRID.md)** – Architecture details
- **[EVENTBRIDGE-ALIGNMENT-GUIDE.md](docs/EVENTBRIDGE-ALIGNMENT-GUIDE.md)** – Production EventBridge setup
- **[PACT-API-QUICK-REFERENCE.md](docs/PACT-API-QUICK-REFERENCE.md)** – Developer cheat sheet (bookmark!)

## What This Module Contains

- Apha.BatchJobs.Api: trigger/status API endpoints.
- Apha.BatchJobs.Worker: job host and runtime orchestration.
- Apha.BatchJobs.Application, Domain, Infrastructure: core business and persistence layers.
- Apha.BatchJobs.UnitTests: unit and integration test coverage.

## Quick Start

From src/Apha.BatchJobs:

```powershell
dotnet build BatchJobs.sln
./test-locally.ps1 -NoPrompt
```

From Linux/macOS:

```bash
dotnet build BatchJobs.sln
./test-locally.sh --no-prompt
```

## Docker Profiles

- withdb: app + postgres.
- nodb: app only, in-memory repositories.

Local compose uses `Dockerfile.local` in this folder.

```bash
docker-compose --profile withdb up --build
docker-compose --profile nodb up --build
```

## Worker Containerization (ECR/ECS)

The production worker image follows the same multi-stage DEFRA pattern used by other root solutions (`Apha.FPS`, `Apha.PACT`, `Apha.PIMS`, `Apha.Costbook`, `Apha.FPSApps`).

- Production Dockerfile: `src/Apha.BatchJobs/Dockerfile`
- Local-only Dockerfile: `src/Apha.BatchJobs/Dockerfile.local`
- CI/ECR build context: repository root (`.`)
- ECR repository: `apha/batchjobs`
- Runtime target: existing ECS/Fargate worker task definition in the same ECS cluster pattern already used by BatchJobs.

Build image (same pattern as CI):

```bash
docker build -f src/Apha.BatchJobs/Dockerfile -t batchjobs-worker:local .
```

Run image locally:

```bash
docker run --rm -e ASPNETCORE_ENVIRONMENT=Development batchjobs-worker:local HealthCheck
```

## Required Runtime Inputs

- ASPNETCORE_ENVIRONMENT: Demo or Development.
- BATCH_JOB_NAME: HealthCheck, ScheduleJobs, FECProcess, or other registered job.
- BATCH_JOBQUEUE_ID: required UUID for strict mode (simulates API/EventBridge trigger id).
- BATCH_USER_ID: optional trigger identity (defaults to system if omitted).
- ConnectionStrings__BatchJobsConnectionString: required for withdb mode.
- BatchJobs__RecreateSummariesImplementationMode: optional and retained for backward compatibility.
	Runtime always uses the LINQ-based RecreateSummaries implementation.
	Retired SQL implementations are preserved under docs/legacy for reference only.
- BATCH_RECREATE_SUMMARIES_MONTH: optional RecreateSummaries month override (0-12).
- BATCH_RECREATE_SUMMARIES_TRIGGERED_BY: optional RecreateSummaries user identity override.

## Local Trigger Simulation (No Cloud Dispatch)

Use this when manually debugging worker flows before API-driven dispatch.

PowerShell example:

```powershell
Set-Location src/Apha.BatchJobs

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:BATCH_JOB_NAME = "MABArchive"
$env:BATCH_RUN_MODE = "AdHoc"
$env:BATCH_JOBQUEUE_ID = [guid]::NewGuid().ToString()
$env:BATCH_USER_ID = "local-debug-user"

dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

Script shortcut:

```powershell
./test-locally.ps1 -Native -JobName MABArchive -NoPrompt
```

## Canonical Docs

- Worker runtime notes: Apha.BatchJobs.Worker/README.md
- Database scripts and operations: database/README.md
- Architecture and deployment summary: docs/README.md
- Codespaces parity and AWS production flow: docs/CODESPACES_PRODUCTION_PARITY_AND_AWS_FLOW.md