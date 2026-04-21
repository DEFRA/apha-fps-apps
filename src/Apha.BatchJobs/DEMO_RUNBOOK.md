# BatchJobs Demo Runbook

## Purpose
This runbook defines the 4-pass demo matrix, required toggles per pass, and the expected console log sequence from program entry to completion.

## Demo Passes

1. Pass 1: Without DB (Local)
- Where: Windows VM local run
- Launch profile: BatchJobs NoDb FirstPass
- Required toggle:
  - BatchJobs__EnableDatabase=false
- Command:
  - dotnet run --project src/Apha.BatchJobs/BatchJobs.csproj --launch-profile "BatchJobs NoDb FirstPass"

2. Pass 2: With local DB (Local)
- Where: Windows VM local run
- Launch profile: BatchJobs WithDb LocalPostgres
- Required toggles:
  - BatchJobs__EnableDatabase=true
  - ConnectionStrings__BatchJobsConnectionString=Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30
- Command:
  - dotnet run --project src/Apha.BatchJobs/BatchJobs.csproj --launch-profile "BatchJobs WithDb LocalPostgres"

3. Pass 3: With just Containers (NoDb, Codespaces)
- Where: GitHub Codespaces
- Required toggle:
  - BatchJobs__EnableDatabase=false (via compose profile nodb)
- Command:
  - docker compose --profile nodb up --build

4. Pass 4: With Containers and container DB (Codespaces)
- Where: GitHub Codespaces
- Required toggles:
  - BatchJobs__EnableDatabase=true (via compose profile withdb)
  - Connection string points to postgres service in compose
- Command:
  - docker compose --profile withdb up --build

## What To Enable Or Change During Demo

1. Flip database mode only through one setting:
- BatchJobs__EnableDatabase
- false for NoDb passes (1 and 3)
- true for WithDb passes (2 and 4)

2. Connection string is only required for WithDb passes:
- Pass 2: localhost connection string
- Pass 4: postgres service connection string

3. Environment consistency:
- DOTNET_ENVIRONMENT=Development
- ASPNETCORE_ENVIRONMENT=Development

4. Job selection controls:
- BATCH_JOB_NAME=HealthCheck
- BATCH_RUN_MODE=AdHoc

## Program Flow (Entry To Completion)

1. Program entry in Apha.BatchJobs.Worker/Program.cs
- Host builder created
- Serilog configured
- Services configured by builder.ConfigureServices()
- Host started

2. Startup flow logs from Program.cs
- Batch Jobs Worker - Starting
- Environment, ProcessId, Timestamp
- Execution mode: WithDb or NoDb
- Flow checkpoint: Program.Main -> Host.Started -> Resolving JobOrchestrator

3. Dispatch to orchestrator
- Program creates service scope
- Resolves IJobOrchestrator
- Calls RunAsync(jobName, runMode, cancellationToken)

4. Orchestrator flow in Apha.BatchJobs.Application/JobOrchestrator.cs
- Orchestrator start log with RunId
- Step 1: acquire lock
- Step 2: create execution record
- Step 3: execute job handler
- Step 4: update execution record
- Step 5: release lock
- Orchestrator finish log with status and duration

5. Job handler flow in Apha.BatchJobs.Application/Jobs/HealthCheck/HealthCheckJobHandler.cs
- HealthCheck Job Started
- Phase 1: configuration and runtime validation
- Phase 2: simulated record processing
- Phase 3: mode-specific path
  - WithDb: database connectivity path log
  - NoDb: in-memory repository path log
- Phase 4: completion report
- HealthCheck Job Completed Successfully

6. Program completion and summary
- Program logs final job status line
- Prints terminal summary line:
  - Run completed | Outcome=... | FailureCategory=... | ExitCode=...
- Host stop and graceful shutdown

## Expected Console Log Sequence (Happy Path)

1. Batch Jobs Worker - Starting
2. Environment: Development
3. Execution mode: NoDb or WithDb
4. Flow checkpoint: Program.Main -> Host.Started -> Resolving JobOrchestrator
5. Requested job: HealthCheck | RunMode: AdHoc
6. --- Orchestrator: Starting 'HealthCheck' | RunId=...
7. Acquiring execution lock for 'HealthCheck'...
8. Lock acquired for 'HealthCheck' | RunId=...
9. Execution record created | ExecutionId=... (WithDb) or RunId=... (NoDb)
10. Executing job 'HealthCheck' | Attempt=1/...
11. === HealthCheck Job Started ===
12. Phase 1: Validating configuration...
13. Phase 2: Processing records...
14. Phase 3: Validating ... path...
15. Phase 4: Job completion report
16. === HealthCheck Job Completed Successfully ===
17. Execution record updated | Status=Completed
18. Lock released for 'HealthCheck' | RunId=...
19. --- Orchestrator: 'HealthCheck' finished | Status=Completed
20. Job 'HealthCheck' finished | Status=Completed
21. Run completed | Outcome=Succeeded | ExitCode=0

## Fast Pass/Fail Criteria

1. Pass if all conditions are true:
- ExitCode=0
- Outcome=Succeeded
- No unhandled exception stack trace
- Program and orchestrator start/finish logs present

2. Additional pass checks for WithDb:
- ExecutionId is greater than 0
- No PostgreSQL connection/timeout errors

3. Additional pass checks for NoDb:
- Execution mode: NoDb is present
- Phase 3 logs mention no-database execution path
