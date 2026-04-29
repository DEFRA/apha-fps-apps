# GitHub Codespaces Parity Guide And AWS Production Flow

This guide gives you:

1. The closest possible test flow in GitHub Codespaces when AWS access is unavailable.
2. The target AWS production flow you will run once access is granted.
3. A side-by-side parity matrix so you know exactly what is validated now versus later.

## Scope

Current implementation detail:

- The API trigger endpoint currently executes jobs in-process through `IJobOrchestrator`.
- It does not yet dispatch ECS tasks or trigger ECR pulls at runtime.

Implication:

- In Codespaces, you can validate almost all application behavior (triggering, locking, execution records, status polling, and HealthCheck completion).
- You cannot validate real ECS task launch and ECR image pull until AWS integration is added and credentials are available.

## Architecture Snapshot

Current runtime path:

1. `POST /api/batch-jobs/{jobName}/trigger` returns `202`.
2. API trigger service starts background execution in the same API host process.
3. Orchestrator acquires lock, writes start row, runs job, writes completion row, releases lock.
4. UI or tester polls status endpoints to observe progress.

Target production runtime path:

1. `POST /api/batch-jobs/{jobName}/trigger` validates request and dispatches external task.
2. ECS launches worker task from task definition.
3. ECS agent pulls image from ECR.
4. Worker runs with `BATCH_JOB_NAME=HealthCheck` and executes orchestrator flow.
5. Status is polled from API and/or logs from CloudWatch.

## Part A: Closest Production-Like Flow In GitHub Codespaces

### Preconditions

1. Docker is available and running in the Codespace.
2. PostgreSQL container is healthy.
3. You are in `src/Apha.BatchJobs`.

### Step 1: Start PostgreSQL (WithDb mode)

Use either VS Code tasks or terminal commands.

Task path:

1. Run task: `debug: postgres up`
2. Run task: `debug: wait postgres healthy`

Terminal path:

```bash
docker compose up -d postgres
docker inspect -f '{{.State.Health.Status}}' batch_jobs_postgres
```

Expected result:

- Container `batch_jobs_postgres` is `running` and `healthy`.

### Step 2: Start API With Development Configuration

Run the API so trigger/status endpoints are available.

```bash
/home/vscode/.dotnet/dotnet run --project Apha.BatchJobs.Api/Apha.BatchJobs.Api.csproj
```

Default URL from launch settings is usually `http://localhost:5261`.

### Step 3: Verify Registered Jobs And Status Endpoints

```bash
curl -s http://localhost:5261/api/batch-jobs | jq
curl -s http://localhost:5261/api/batch-jobs/HealthCheck/status | jq
curl -s http://localhost:5261/api/batch-jobs/HealthCheck/can-run | jq
```

Expected result:

- `HealthCheck` appears in job list.
- `canRun` is true before triggering (unless another run is active).

### Step 4: Trigger HealthCheck Through API

```bash
curl -s -X POST http://localhost:5261/api/batch-jobs/HealthCheck/trigger | jq
```

Expected result:

- HTTP `202 Accepted`.
- Response contains `operationId`, `acceptedAt`, and `jobName`.

### Step 5: Poll For Completion

```bash
for i in {1..30}; do
  curl -s http://localhost:5261/api/batch-jobs/HealthCheck/status | jq
  sleep 2
done
```

Expected result:

1. `isRunning` flips to true shortly after trigger.
2. Later, `isRunning` becomes false.
3. `lastExecution.status` becomes `Completed` (or meaningful failure state).

### Step 6: Validate Concurrency Protection (Lock Behavior)

Trigger twice quickly:

```bash
curl -i -X POST http://localhost:5261/api/batch-jobs/HealthCheck/trigger
curl -i -X POST http://localhost:5261/api/batch-jobs/HealthCheck/trigger
```

Expected result:

- First call: `202 Accepted`.
- Second call during active run: `409 Conflict` with reason `Job is already running`.

### Step 7: Optional Database Evidence Check

Use psql in container:

```bash
docker exec -it batch_jobs_postgres psql -U postgres -d batch_jobs_foundation_db
```

Inspect queue and lock-related tables to confirm start/end record lifecycle.

### Why This Is The Closest Parity In Codespaces

This validates the same domain and persistence behavior production depends on:

- Endpoint contract
- Job selection (`BATCH_JOB_NAME` semantics via orchestrator path)
- Distributed locking semantics
- Execution tracking lifecycle
- Status polling behavior

The only missing layer is AWS orchestration itself (ECS `RunTask` + ECR pull + VPC/IAM/network).

## Part B: Actual AWS Production Flow (When Access Is Available)

### High-Level Sequence

1. UI calls API trigger endpoint.
2. API validates request and dispatches to ECS.
3. ECS schedules task in target cluster/subnets/security groups.
4. ECS agent pulls container image from ECR.
5. Worker starts, reads environment (`BATCH_JOB_NAME=HealthCheck`, `BATCH_RUN_MODE=Manual`).
6. Worker orchestrator executes lock and persistence lifecycle against PostgreSQL.
7. Logs and metrics flow to CloudWatch.
8. API status endpoint reports final state.

### Required AWS Resources

1. ECR repository with pushed worker image.
2. ECS cluster and task definition.
3. Networking: subnets, security groups, route/NAT policy as required.
4. IAM:
   - API role with `ecs:RunTask` and `iam:PassRole`.
   - Task execution role with ECR pull and CloudWatch write permissions.
5. Data store reachable from ECS task runtime.

### Required Runtime Inputs For Worker Task

1. `ASPNETCORE_ENVIRONMENT=Production`
2. `BATCH_JOB_NAME=HealthCheck`
3. `BATCH_RUN_MODE=Manual`
4. `ConnectionStrings__BatchJobsConnectionString` (or secret reference)

### Production Validation Checklist

1. Trigger returns `202` with correlation id.
2. ECS task appears in running tasks for expected cluster/service.
3. Task logs show requested job and run mode.
4. Job completion status is persisted.
5. API status endpoint transitions from running to completed/failed.
6. Duplicate trigger during active run returns `409`.

## Parity Matrix

| Capability | GitHub Codespaces | AWS Production |
|---|---|---|
| API endpoint contract | Yes | Yes |
| HealthCheck trigger call | Yes | Yes |
| Locking and execution persistence | Yes | Yes |
| Status polling behavior | Yes | Yes |
| Worker process execution | Yes (in-process from API trigger path) | Yes (container task) |
| ECS task scheduling | No | Yes |
| ECR image pull at runtime | No | Yes |
| IAM, VPC, SG validation | No | Yes |
| CloudWatch task logs | Partial/local logs only | Yes |

## Recommended Testing Strategy Until AWS Access Is Available

1. Run all trigger/status/locking tests in Codespaces with PostgreSQL.
2. Capture baseline expected API responses and status transitions.
3. Keep a short promotion checklist for AWS-only verifications:
   - ECS task launch
   - ECR pull
   - IAM pass-role permissions
   - Network reachability to DB

This split gives high confidence in business behavior now and isolates AWS integration risk to a smaller later test window.
