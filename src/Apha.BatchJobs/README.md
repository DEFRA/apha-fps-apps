# Apha.BatchJobs

Short reference for running, testing, and operating the Batch Jobs foundation.

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

```bash
docker-compose --profile withdb up --build
docker-compose --profile nodb up --build
```

## Required Runtime Inputs

- ASPNETCORE_ENVIRONMENT: Demo or Development.
- BATCH_JOB_NAME: HealthCheck, ScheduleJobs, FECProcess, or other registered job.
- ConnectionStrings__BatchJobsConnectionString: required for withdb mode.
- BatchJobs__RecreateSummariesImplementationMode: optional and retained for backward compatibility.
	Runtime always uses the LINQ-based RecreateSummaries implementation.
	Retired SQL implementations are preserved under docs/legacy for reference only.
- BATCH_RECREATE_SUMMARIES_MONTH: optional RecreateSummaries month override (0-12).
- BATCH_RECREATE_SUMMARIES_TRIGGERED_BY: optional RecreateSummaries user identity override.

## Canonical Docs

- Worker runtime notes: Apha.BatchJobs.Worker/README.md
- Database scripts and operations: database/README.md
- Architecture and deployment summary: docs/README.md
- Codespaces parity and AWS production flow: docs/CODESPACES_PRODUCTION_PARITY_AND_AWS_FLOW.md