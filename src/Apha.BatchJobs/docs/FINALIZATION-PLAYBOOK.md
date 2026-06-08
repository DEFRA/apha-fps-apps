# BatchJobs End-to-End Finalization Playbook

Use this as the single briefing guide when speaking to DevOps, PACT, FPS, QA, and business stakeholders.

## 1. The 3 Documents To Use

1. [api-to-eventbridge-ecs-batch-trigger-updated.html](api-to-eventbridge-ecs-batch-trigger-updated.html)
   - Primary architecture and integration contract.
   - Includes EventBridge -> ECS mapping, IAM expectations, and operational guidance.

2. [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md)
   - Timeline-style scenarios from happy path to timeout/failure behavior.
   - Best for walkthrough sessions and QA/UAT demonstrations.

3. [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml)
   - Exact API contract for PACT/FPS callers.
   - Best for dev teams implementing or validating request/response behavior.

## 2. How To Explain The End-to-End Flow

### Manual/API Trigger Flow (Cloud)

1. Caller (PACT or FPS API) sends trigger request.
2. API validates request and route policy, then creates `jobExecutionId`.
3. API publishes `BatchJob.TriggerRequested` event to EventBridge (`source` = `pact.api` or `fps.api`).
4. EventBridge rule maps detail fields into ECS container environment variables.
5. ECS Fargate starts BatchJobs worker.
6. Worker writes execution state to BatchJobs DB and runs job.
7. Status/caller polling uses `jobExecutionId` correlation for tracking.

### Scheduled Job Flow (Cloud)

1. EventBridge schedule rule triggers at configured time (for example MABArchive).
2. Rule invokes ECS task directly.
3. Input transformer maps top-level `$.id` to `BATCH_JOB_EXECUTION_ID`.
4. Rule sets fixed job metadata (for example `BATCH_JOB_NAME=MABArchive`, `BATCH_RUN_MODE=Scheduled`).
5. Worker runs, writes state, and logs to CloudWatch.

## 3. Happy Path To Worst Case (Stakeholder Script)

### Happy Path

1. Trigger accepted (202 for manual/API).
2. Worker starts and writes `Pending` then `Running`.
3. Job completes and writes terminal `Completed`.
4. Polling/status shows clean transition with same `jobExecutionId`.

### Degraded But Recoverable

1. Trigger accepted but worker start is delayed.
2. Watchdog/transient status remains in startup phase.
3. Worker appears before SLA deadline and continues normally.

### Worst Case

1. Trigger accepted but worker never appears before SLA deadline.
2. Status projects startup timeout (manual/API path).
3. Or EventBridge target invocation fails and lands in DLQ.
4. CloudWatch alarm fires on DLQ visible messages.
5. SNS notification alerts Ops/DevOps for investigation.

## 4. PACT/FPS Team Contract (How To Call Batch Worker In Cloud)

### Canonical API Endpoints

- `GET /health`
- `GET /api/v1/batch-jobs/catalog`
- `POST /api/v1/batch-jobs/trigger`
- `GET /api/v1/batch-jobs/{jobName}/can-run`
- `GET /api/v1/batch-jobs/{jobName}/status?jobExecutionId={guid}`
- `POST /api/v1/batch-jobs/{jobName}/cancel`

### Event Envelope Requirements

- `source`: `pact.api` or `fps.api`
- `detail-type`: `BatchJob.TriggerRequested`
- `detail` fields:
  - `jobExecutionId`
  - `jobName`
  - `runMode`
  - `requestedBy`
  - `requestedAtUtc`

### ECS Container Variables Required By Worker

- `BATCH_JOB_EXECUTION_ID`
- `BATCH_JOB_NAME`
- `BATCH_RUN_MODE`
- `BATCH_REQUESTED_BY`

### Non-Negotiables

1. `jobExecutionId` must be unique per accepted trigger.
2. Keep event detail field names lower camel case.
3. Production must run with EventBridge dry-run disabled.
4. Status tracking should use `jobExecutionId` query correlation; latest-by-job-name is fallback only.
5. Startup watchdog SLA is 30 seconds in Dev/Local and 600 seconds in Production.
