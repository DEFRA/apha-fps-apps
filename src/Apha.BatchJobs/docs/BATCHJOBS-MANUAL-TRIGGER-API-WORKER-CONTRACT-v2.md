# BatchJobs Manual Trigger API-Worker Contract (v2)

## 0. Executive Summary

This document defines the contract between PACT API, FPS API, EventBridge, and BatchJobs Worker for coordinating manually triggered (and scheduled) batch job execution. The primary goal is deterministic job correlation via `jobExecutionId` across all components from API trigger through worker completion.

**Key points for reviewers:**

- **API teams**: Implement trigger, status, cancel, and can-run endpoints. Publish events with required fields. Consider idempotency (Section 13.1).
- **Worker team**: Read env vars, acquire lock, create execution row, and release lock. Poll for cancellation requests.
- **DevOps/Architect**: EventBridge rules are pre-configured. Discuss cache-first vs database-first trade-offs (Section 13.3) if future changes needed.
- **DBA**: Maintain unique partial index on `fps.job_lock`. Monitor and tune per production load (Section 14.4).

**Current state:** Proposed for team review and cross-team sign-off. Implementation is ongoing; gaps are marked as "target-state."

**Table of Contents:**
1. [Document Control](#1-document-control)
2. [Purpose and Boundary](#2-purpose-and-contract-boundary)
3. [Caller Identity](#3-caller-identity-and-trust-boundary-normative)
4. [Manual Trigger Flow](#4-manual-trigger-flow)
5. [API Contract](#5-api-contract)
6. [EventBridge Contract](#6-eventbridge-contract)
7. [Payload Limits](#7-payload-size-and-validation-limits)
8. [State Contract](#8-state-contract-for-ui)
9. [Database Contract](#9-database-contract)
10. [Implementation Notes](#10-implementation-notes-corrected)
11. [Security and IAM](#11-security-and-iam)
12. [Acceptance Criteria](#12-acceptance-criteria)
13. [Next Iteration Considerations](#13-implementation-considerations-for-next-iteration)
14. [Team-Specific Guidance](#14-team-specific-guidance)
15. [Known Gaps and Risks](#15-known-gaps-and-risks)
16. [Sign-off](#16-sign-off)

---

## 1. Document Control

- Audience: PACT API Team, FPS API Team, BatchJobs Worker Team, DevOps
- Purpose: Define the API, event, state, database, and implementation contract for manually triggered BatchJobs
- Runtime path: UI -> PACT/FPS API -> Amazon EventBridge Rule -> ECS Fargate BatchJobs Worker -> BatchJobs DB
- Primary correlation key: `jobExecutionId` (GUID), returned to UI and persisted as `fps.job_queue.jobexecutionid`
- Event detail type: `BatchJobTriggerRequested`
- Allowed event sources: `fps.api`, `pact.api`
- Status: Contract draft for team review and sign-off

Implementation alignment note (2026-06-11):
- This document contains both target-state contract rules and current implementation behavior where they differ.
- Any item marked as "target-state" requires API work before formal cross-team sign-off.

This contract is primarily scoped to manual/API-triggered execution and also includes scheduled-job operational paths for failure monitoring and support runbooks.

Normative language:
- The key words MUST, MUST NOT, SHOULD, SHOULD NOT, and MAY in this document are to be interpreted as described in RFC 2119.

---

## 2. Purpose and Contract Boundary

This contract defines what PACT API and FPS API must implement so that a user-triggered batch job can start BatchJobs Worker through EventBridge and be tracked consistently by UI.

- BatchJobs Worker is the execution layer.
- PACT/FPS APIs are the trigger and status surfaces.
- EventBridge is the integration boundary between API and Worker.

### 2.1 Ownership Matrix

| Area | Owner | Contract responsibility |
|---|---|---|
| UI | PACT/FPS consuming UI | Submit trigger, display returned `jobExecutionId`, poll status by `jobExecutionId`, display user-facing state |
| PACT API / FPS API | PACT/FPS team | Validate request, generate `jobExecutionId`, supply non-empty `requestedBy` from caller context, publish event, expose status/can-run/cancel endpoints |
| EventBridge | DevOps / Platform | Match event pattern and invoke ECS task with required container environment variables |
| BatchJobs Worker | BatchJobs team | Read `BATCH_*` env vars, enforce lock, create/update execution rows, execute job, write status/log |
| BatchJobs DB | BatchJobs team | Authoritative persistent execution state |

Non-goals:
- EventBridge Scheduler configuration
- UI styling details
- internal SQL-to-.NET mapping implementation details
- full operations runbook

---

## 3. Caller Identity and Trust Boundary (Normative)

1. This handoff does not require a full authentication/authorization implementation in caller APIs.
2. Caller API MUST supply a non-empty `requestedBy` value in trigger/cancel operations.
3. `requestedBy` is treated as caller-provided audit provenance for execution history.
4. Worker does not resolve identity; it persists provided `requestedBy` for traceability.
5. API MUST publish events only with approved source values (`pact.api` or `fps.api`).
6. Event source allow-list and deterministic `jobExecutionId` are the required trust boundary for this integration.

---

## 4. Manual Trigger Flow

1. User triggers job from UI.
2. API validates route policy and request payload.
3. API sets `requestedBy` from caller context (authenticated claim when available, otherwise caller-supplied identity string).
4. API applies idempotency policy where implemented; otherwise each accepted POST is treated as a new trigger (see Section 5.4 and 13.1).
5. API generates `jobExecutionId` (GUID) for new accepted trigger.
6. API publishes `BatchJobTriggerRequested` event.
7. EventBridge rule invokes ECS task with container overrides.
8. Worker reads env vars and begins orchestration.
9. Worker writes `Pending` then `Running` and terminal states to DB.
10. UI polls status endpoint by `jobExecutionId` until terminal.

## 4.1 End-to-End Path Catalogue

### 4.1.1 Manual/API-triggered Jobs

#### A) Happy path (all components healthy)
1. UI calls trigger endpoint on PACT API or FPS API.
2. API validates request, routing policy, idempotency, and caller context.
3. API generates `jobExecutionId`, sets `requestedAtUtc`, and publishes `BatchJobTriggerRequested` to EventBridge.
4. EventBridge manual rule matches (`source` in `pact.api|fps.api`) and invokes ECS `RunTask` for `FPSBatchJobs`.
5. ECS injects mapped env vars into container (`BATCH_JOB_EXECUTION_ID`, `BATCH_JOB_NAME`, `BATCH_RUN_MODE`, `BATCH_REQUESTED_BY`, `BATCH_REQUESTED_AT_UTC`, optional `BATCH_JOB_PARAMETERS_JSON`).
6. Worker starts, validates env contract, acquires lock, and writes execution rows to `fps.job_queue` and `fps.job_queue_log`.
7. Worker processes job and transitions status to terminal `Completed`.
8. UI polls by `jobExecutionId` and receives deterministic terminal success.

Where to observe:
- API logs for accepted trigger and EventBridge publish metadata.
- Worker container logs in CloudWatch (`devldnaph-app-fpsbatchjobs`).
- DB status in `fps.job_queue` and `fps.job_queue_log`.

#### B) Worker-side processing exception (task started, job failed)
1. API publish and EventBridge invocation succeed.
2. ECS task and container start successfully.
3. Worker throws during execution (business/runtime/dependency failure).
4. Worker updates execution state to terminal `Failed` and persists error context.
5. UI polling returns `Failed` for the same `jobExecutionId`.

Where to observe:
- CloudWatch container logs (stack trace, exception category).
- ECS task state change alarms/runtime failure alarms.
- `fps.job_queue.errormessage`, terminal status, and timestamps.

#### C) Worker did not start at all (invocation failure before runtime)
1. API publish succeeds.
2. EventBridge cannot complete ECS target invocation (for example `RunTask`/`PassRole`/network/task-definition issues).
3. EventBridge retries per policy (`max age`, `retry attempts`), then writes event to manual DLQ (`devldnaph-batch-manual-dlq`).
4. No worker process starts, so no execution row is created.
5. UI status path eventually resolves as startup timeout/no-row-visible condition.

Where to observe:
- EventBridge failed invocation metrics.
- SQS manual DLQ payload/metadata.
- DLQ CloudWatch alarm and SNS notification.

#### D) API publish failed (event never entered EventBridge target path)
1. UI calls trigger endpoint.
2. API fails `events:PutEvents` (permission, bus issue, transient AWS error, or payload problem).
3. API returns dispatch failure (`DispatchFailed`, retryable).
4. No EventBridge target invocation occurs, no ECS task starts, and no worker execution row exists.

Where to observe:
- PACT/FPS API logs (`FailedEntryCount`, `ErrorCode`, `ErrorMessage`, `EventId` when present).
- API CloudWatch application alarms for publish failures.

### 4.1.2 Scheduled Jobs (MABArchive)

#### A) Happy path (all components healthy)
1. EventBridge Scheduler fires at configured cron (`cron(0 20 ? * MON-FRI *)`).
2. Scheduler invokes ECS `RunTask` target with container overrides for scheduled run.
3. Container env includes `BATCH_JOB_NAME=MABArchive`, `BATCH_RUN_MODE=Scheduled`, `BATCH_REQUESTED_BY=EventBridgeScheduler`, and `BATCH_REQUESTED_AT_UTC=<aws.scheduler.scheduled-time>` (recommended for startup latency observability).
4. Worker starts and persists execution.
5. If `BATCH_REQUESTED_AT_UTC` is provided, worker parses and persists it to `requested_at_utc`. If absent, worker MAY default to `DateTime.UtcNow` at startup, or leave as null for scheduler-only runs where latency measurement is not needed.
6. Job runs and reaches terminal `Completed`.

Where to observe:
- CloudWatch logs for scheduler-invoked worker container.
- DB rows in `fps.job_queue`/`fps.job_queue_log`.

#### B) Worker-side processing exception (task started, job failed)
1. Scheduler invocation and ECS startup succeed.
2. Worker fails during execution and writes terminal `Failed`.
3. Runtime alarms trigger SNS notification.

Where to observe:
- CloudWatch worker logs.
- ECS task state change alarms.
- Terminal DB state in `fps.job_queue`.

#### C) Worker did not start at all (scheduler invocation failure)
1. Schedule fires but Scheduler cannot invoke ECS target.
2. Retries are attempted per scheduler retry policy.
3. After retries/max age, invocation is written to scheduled DLQ (`devldnaph-batch-schedule-dlq`).
4. No worker runtime and no execution row for that attempt.

Where to observe:
- Scheduler/EventBridge failure metrics.
- Scheduled DLQ messages.
- DLQ alarm and SNS notification.

#### D) Scheduled publish-failure equivalent
1. There is no API `PutEvents` hop in the scheduled path.
2. The equivalent failure class is Scheduler-to-ECS invocation failure (captured in scheduled DLQ and alarms).
3. If Scheduler successfully invokes ECS but container fails later, this is runtime failure (not DLQ path).

### 4.1.3 Failure Domain Summary

| Failure class | Manual/API path | Scheduled path | Primary evidence source |
|---|---|---|---|
| Publish failure before EventBridge target invocation | API `PutEvents` failure | Not applicable | API logs + API alarms |
| EventBridge/Scheduler cannot invoke ECS target | Manual rule target failure -> manual DLQ | Scheduler target failure -> scheduled DLQ | SQS DLQ + EventBridge/Scheduler metrics + alarms |
| ECS task/container starts but job fails | Runtime failure | Runtime failure | ECS task state + CloudWatch logs + DB terminal state |

---

## 5. API Contract

## 5.1 Endpoints

| Method | Endpoint | Required | Purpose |
|---|---|---|---|
| GET | `/health` | MAY | Liveness check |
| GET | `/api/v{version}/batch-jobs/{jobName}/can-run` | SHOULD | Advisory pre-check |
| POST | `/api/v{version}/batch-jobs/trigger` | MUST | Accept trigger and publish event |
| GET | `/api/v{version}/batch-jobs/{jobName}/status?jobExecutionId={guid}` | MUST | Deterministic status by correlation key |
| POST | `/api/v{version}/batch-jobs/{jobName}/{jobExecutionId}/cancel` | SHOULD | Deterministic idempotent cancellation |

Catalog endpoint exposure:
1. `GET /api/v{version}/batch-jobs/catalog` is not part of this external handoff contract.
2. APIs MAY keep a private/internal catalog endpoint for diagnostics, but UIs and external callers MUST NOT depend on it.

## 5.2 Trigger Request

`POST /api/v{version}/batch-jobs/trigger`

Headers:
- `Idempotency-Key` (SHOULD): unique key per client intent (UUID recommended)

Body:

```json
{
  "jobName": "RecreateSummaries",
  "requestedBy": "user.name@defra.gov.uk",
  "parametersJson": "{\"month\":\"2026-06\"}"
}
```

Rules:
1. `jobName` is required and MUST be non-empty.
2. `parametersJson` is optional JSON string and MUST contain a valid JSON object that satisfies job-specific schema.
3. `requestedBy` is required and MUST be non-empty.
4. Caller API SHOULD map `requestedBy` from authenticated identity where available, but this is not a handoff prerequisite.
5. Caller API SHOULD implement idempotency via `Idempotency-Key` to improve retry safety.

## 5.3 Trigger Response (Accepted)

```json
{
  "accepted": true,
  "source": "pact.api",
  "jobName": "RecreateSummaries",
  "jobExecutionId": "7f9d2f2e-8d1b-4f7a-9d25-6d6e8a9f3c12",
  "eventId": "aws-event-id",
  "status": "TriggerAccepted",
  "acceptedAtUtc": "2026-06-11T10:20:00Z",
  "effectiveRequestedBy": "user.name@defra.gov.uk",
  "message": "Trigger accepted for dispatch."
}
```

## 5.4 Idempotency Rules

1. API SHOULD require `Idempotency-Key` on trigger requests.
2. API SHOULD persist dedupe records for at least 24 hours.
3. Duplicate trigger with the same key and same effective request payload SHOULD return the original acceptance response (`202`) and same `jobExecutionId`.
4. Duplicate key with different payload SHOULD return `409 IdempotencyConflict`.

## 5.5 Cancellation Rules

Endpoint (current): `POST /api/v{version}/batch-jobs/{jobName}/cancel` with `jobExecutionId` in request body

Endpoint (target-state): `POST /api/v{version}/batch-jobs/{jobName}/{jobExecutionId}/cancel`

Rules:
1. Cancellation target MUST be deterministic by `jobExecutionId`.
2. Repeated cancel requests MUST be idempotent.
3. If execution is already terminal, API SHOULD return success with terminal state and a no-op message.
4. Worker MUST remain final authority for cooperative cancellation points.

## 5.6 HTTP and Error Envelope

All non-2xx responses MUST use:

```json
{
  "error": {
    "code": "ValidationFailed",
    "reason": "jobName is required",
    "correlationId": "c8a6e9fb-70e2-4f76-a71b-1f9d8f0f65d6",
    "retryable": false,
    "details": {}
  }
}
```

Status mapping:

| Case | HTTP | code | retryable |
|---|---|---|---|
| Missing/invalid payload | 400 | `ValidationFailed` | false |
| Unauthorized/forbidden | 401/403 | `Unauthorized` / `Forbidden` | false |
| Job blocked by route policy | 409 | `RoutingPolicyConflict` | false |
| Idempotency key conflict | 409 | `IdempotencyConflict` | false |
| Active lock / already running | 409 | `AlreadyRunning` or `LockConflict` | true |
| EventBridge dispatch failure | 503 | `DispatchFailed` | true |
| Status dependency unavailable | 503 | `DependencyFailure` | true |

---

## 6. EventBridge Contract

### 6.1 Event pattern

```json
{
  "source": ["fps.api", "pact.api"],
  "detail-type": ["BatchJobTriggerRequested"]
}
```

### 6.2 Example event payload

```json
{
  "source": "pact.api",
  "detail-type": "BatchJobTriggerRequested",
  "detail": {
    "jobExecutionId": "7f9d2f2e-8d1b-4f7a-9d25-6d6e8a9f3c12",
    "jobName": "RecreateSummaries",
    "runMode": "Manual",
    "requestedBy": "user.name@defra.gov.uk",
    "requestedAtUtc": "2026-06-09T13:41:27Z",
    "parametersJson": "{\"month\":\"2026-06\"}"
  }
}
```

`jobExecutionId` representation:
1. Current implementation returns compact GUID string (`N` format, no hyphens).
2. Worker accepts valid GUID values in either compact or dashed representation.
3. Target-state contract recommends canonical dashed (`D`) representation in external API responses and examples.

`requestedAtUtc` field:
1. Present for manual/API-triggered jobs. The API sets this to `DateTime.UtcNow` at acceptance time and publishes it in the event detail.
2. Worker reads `BATCH_REQUESTED_AT_UTC` as optional input, parses it as UTC, and persists it to `fps.job_queue.requested_at_utc` when valid.
3. For scheduled runs (e.g. MABArchive via EventBridge Scheduler):
   - EventBridge Scheduler SHOULD pass `BATCH_REQUESTED_AT_UTC` using the scheduler fire time for observability.
   - If `BATCH_REQUESTED_AT_UTC` is provided, worker persists the scheduler's timestamp.
   - If `BATCH_REQUESTED_AT_UTC` is **not** provided, worker MAY default to `DateTime.UtcNow` (worker startup time) or leave null depending on job requirements. See Section 9.3 timestamp semantics for field handling.
4. `MABArchive` is `ScheduledOnly` in the routing policy and cannot be triggered via PACT or FPS API. Any API trigger attempt returns `409 RoutingPolicyConflict`.

### 6.3 Input transformer mapping

| Event detail field | Input path | Worker env var |
|---|---|---|
| `jobExecutionId` | `$.detail.jobExecutionId` | `BATCH_JOB_EXECUTION_ID` |
| `jobName` | `$.detail.jobName` | `BATCH_JOB_NAME` |
| `runMode` | `$.detail.runMode` | `BATCH_RUN_MODE` |
| `requestedBy` | `$.detail.requestedBy` | `BATCH_REQUESTED_BY` |
| `requestedAtUtc` | `$.detail.requestedAtUtc` | `BATCH_REQUESTED_AT_UTC` (manual triggers required; recommended for scheduled runs) |
| `parametersJson` | `$.detail.parametersJson` | `BATCH_JOB_PARAMETERS_JSON` |

Input paths map:

```json
{
  "jobExecutionId": "$.detail.jobExecutionId",
  "jobName": "$.detail.jobName",
  "runMode": "$.detail.runMode",
  "requestedBy": "$.detail.requestedBy",
  "requestedAtUtc": "$.detail.requestedAtUtc",
  "parametersJson": "$.detail.parametersJson"
}
```

### 6.4 Input template / container override example

```json
{
  "containerOverrides": [
    {
      "name": "FPSBatchJobs",
      "environment": [
        { "name": "BATCH_JOB_EXECUTION_ID", "value": "<jobExecutionId>" },
        { "name": "BATCH_JOB_NAME", "value": "<jobName>" },
        { "name": "BATCH_RUN_MODE", "value": "<runMode>" },
        { "name": "BATCH_REQUESTED_BY", "value": "<requestedBy>" },
        { "name": "BATCH_REQUESTED_AT_UTC", "value": "<requestedAtUtc>" },
        { "name": "BATCH_JOB_PARAMETERS_JSON", "value": "<parametersJson>" }
      ]
    }
  ]
}
```

---

## 7. Payload Size and Validation Limits

1. Trigger payload (including `parametersJson`) MUST be <= 64 KB.
2. Event detail MUST remain under EventBridge entry limits.
3. `BATCH_JOB_PARAMETERS_JSON` SHOULD be <= 4 KB for safety.
4. If `parametersJson` exceeds configured limit, API MUST persist parameters in durable storage and pass a reference key/URI in event detail.
5. API MUST validate that `parametersJson` is parseable JSON and satisfies per-job parameter schema before publish.

### 7.1 BATCH_JOB_PARAMETERS_JSON requirements

Purpose:
- Optional job-specific payload passed to the worker as one JSON object string.

Validation rules:
1. If supplied, it must be valid JSON.
2. Root type must be a JSON object.
3. Invalid payload causes worker startup failure.

Valid examples:
1. `{}`
2. `{"month":"2026-06"}`
3. `{"month":"2026-06","dryRun":false}`
4. `{"fromDate":"2026-06-01","toDate":"2026-06-30","includeArchived":true}`

Invalid examples:
1. `[]`
2. `"month=2026-06"`
3. `123`
4. `true`
5. `{"month":6}` for `RecreateSummaries`

RecreateSummaries rule:
1. `month` is required.
2. `month` format must be `YYYY-MM`.
3. Example: `{"month":"2026-06"}`

---

## 8. State Contract for UI

## 8.1 States

| State | Source | Terminal | Meaning |
|---|---|---|---|
| `TriggerAccepted` | API watchdog | No | Accepted but worker row not visible yet |
| `WorkerProcessStarted` | API projection/local dev | No | Worker started but row not visible |
| `StartFailedTimeout` | API watchdog | Yes | Worker row not visible within startup SLA |
| `Pending` | DB | No | Execution queued/preparing |
| `Running` | DB | No | Execution active |
| `Retry` | DB | No | Retrying |
| `Completed` | DB | Yes | Success |
| `Failed` | DB | Yes (unless new retry execution starts) | Failure |
| `Cancelled` | DB | Yes | Cancelled |
| `Skipped` | DB or trigger-time rejection | Yes | Skipped/rejected |

## 8.2 Polling and Watchdog Rules

- Startup polling interval: 2-5 seconds
- Running polling interval: 15-30 seconds
- Retry polling interval: 2-5 seconds, then normal
- Stop polling on terminal state

Startup SLA (normative):
1. Current implementation evaluates startup timeout at 600 seconds in Production and 30 seconds outside Production.
2. Target-state may move to config-driven values, but contract and implementation must remain aligned.
3. Before timeout and before DB row appears, status MAY report projected state (`TriggerAccepted` or `WorkerProcessStarted`).
4. After timeout with no DB row, status MUST report `StartFailedTimeout`.

Status response MUST include:
- `sourceOfTruth`: `StartupWatchdog` or `BatchJobs`
- `correlatedJobExecutionId`
- `lastExecution` (when DB row exists)
- `startupWatchdog` object when projection state is active

Status query rule:
1. Contract requires deterministic correlation by valid `jobExecutionId`.
2. Status endpoint MUST reject missing or invalid `jobExecutionId` with `400 ValidationFailed` for deterministic UI tracking.

---

## 9. Database Contract

## 9.1 Primary Tables

| Table | Usage |
|---|---|
| `fps.job_master` | Catalog and route policy metadata |
| `fps.job_status` | Status definitions |
| `fps.job_queue` | Primary execution state by `jobExecutionId` |
| `fps.job_queue_log` | Execution timeline/audit |
| `fps.job_lock` | Active lock diagnostics and can-run advisory |

## 9.2 Key Fields

- `fps.job_queue.jobexecutionid`: GUID correlation key
- `fps.job_queue.jobqueueid`: queue row ID (UUID in current schema)

APIs MUST treat `jobqueueid` as UUID.

## 9.3 Recommended Status Query

```sql
SELECT
    jq.jobqueueid,
    jq.jobexecutionid,
    jm.jobname,
    js.status,
    jq.requestedby,
    jq.requested_at_utc,
    jq.startdatetime AS started_at,
    jq.enddatetime AS ended_at,
    jq.errormessage,
    jq.created_at AS record_created_at,
    jq.updated_at AS record_updated_at
FROM fps.job_queue jq
JOIN fps.job_master jm ON jm.jobid = jq.jobid
JOIN fps.job_status js ON js.statusid = jq.statusid
WHERE jq.jobexecutionid = @jobExecutionId
  AND jm.jobname = @jobName
ORDER BY jq.created_at DESC
LIMIT 1;
```

Timestamp semantics in `fps.job_queue`:

| Column | Nullable | Set by | Set when |
|---|---|---|---|
| `startdatetime` | No | Worker on `CreateExecutionRecord` | Worker starts and inserts the first queue row (maps to `record.StartedAt`) |
| `enddatetime` | Yes | Worker on `UpdateExecutionRecord` | Worker writes terminal or update state (maps to `record.CompletedAt`); null while running |
| `requested_at_utc` | Yes | API on trigger accept, then worker on `CreateExecutionRecord` | API emits `requestedAtUtc` for manual triggers; worker parses `BATCH_REQUESTED_AT_UTC` and persists when provided/valid. For scheduled runs, populated only if `BATCH_REQUESTED_AT_UTC` was passed by scheduler. |
| `created_at` | No | Worker on insert (`DateTime.UtcNow`) | Row is first inserted; never changed after that |
| `updated_at` | No | Worker on every update (`DateTime.UtcNow`) | Updated on every `UpdateExecutionRecord` call (status changes, error writes, etc.) |

Key distinctions:
- `requested_at_utc` is trigger acceptance time from API (manual runs) or scheduler fire time (scheduled runs). It is null when not provided (for example scheduled runs without `BATCH_REQUESTED_AT_UTC`) or if the incoming value is invalid.
- `startdatetime` is the job execution start time (business event); `created_at` is when the row was inserted into the DB (infrastructure event). They are the same timestamp in practice today because the worker inserts the row and starts the job in one operation, but they have different semantic meanings.
- `enddatetime` is null until the job finishes. `updated_at` changes on every status update, including intermediate ones like `Running` → cancellation log entries.
- Use `enddatetime - startdatetime` to calculate actual job duration. Use `updated_at` for cache freshness or stale-row detection. Use `startdatetime - requested_at_utc` (when present) to measure trigger-to-worker startup latency.

`requestedAtUtc` persistence (current behavior):
- `requestedAtUtc` is passed in the EventBridge event detail and received by the worker as `BATCH_REQUESTED_AT_UTC`.
- Worker parses this value as UTC and persists it into `fps.job_queue.requested_at_utc` on `CreateExecutionRecord`.
- Invalid timestamp values are ignored (stored as null) and logged as a warning.

Cannot be mapped to `created_at`:
- `created_at` = `DateTime.UtcNow` when the worker calls `CreateExecutionRecordAsync` inside `JobOrchestrator.RunAsync` — this is after ECS task launch, container boot, and worker startup.
- `requestedAtUtc` = `DateTime.UtcNow` when the API accepted the trigger — this is before EventBridge dispatch, before ECS cold start, before the worker starts.
- The gap is typically 15–60+ seconds in production (ECS Fargate cold start). They are not equivalent and `created_at` MUST NOT be used as a substitute for `requestedAtUtc`.

Still cannot be mapped to `startdatetime`:
- `startdatetime` maps to `record.StartedAt` which is also set inside `JobOrchestrator.RunAsync` at the same point as `created_at`.
- In practice `startdatetime ≈ created_at` today (both set at orchestrator entry). They should remain execution timestamps, not trigger-accept timestamps.

Implemented design change:
1. `fps.job_queue` includes `requested_at_utc TIMESTAMPTZ NULL`.
2. `JobExecutionRecord` includes `RequestedAtUtc`.
3. Worker parses `BATCH_REQUESTED_AT_UTC` and passes it to orchestrator.
4. Repository persists `RequestedAtUtc` into `requested_at_utc`.

Naming guidance:
- Current schema column names are preserved for compatibility.
- Follow existing snake_case timestamp pattern for persisted DB columns (`requested_at_utc`, `created_at`, `updated_at`).
- For read models and API responses, alias as `started_at`, `ended_at`, `record_created_at`, `record_updated_at` to make the intent explicit.

## 9.4 Actual Database Design (Complete Inventory)

Source of truth:
- The inventory below is derived from the current EF mapping in `Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs`.
- Object type is included because BatchJobs uses both base tables and database views.

### 9.4.1 Execution and Control Objects (fps schema)

| Object | Type | Used by |
|---|---|---|
| `fps.job_master` | Table | Job catalog and route metadata |
| `fps.job_status` | Table | Status vocabulary per job |
| `fps.job_queue` | Table | Authoritative execution instance state |
| `fps.job_queue_log` | Table | Execution timeline/audit entries |
| `fps.job_lock` | Table | Single-flight lock and contention control |
| `fps.job_cancellation_request` | Table | Durable idempotent cancellation requests |

### 9.4.2 RecreateSummaries Objects (fps schema)

Tables:
- `fps.fpsyeartotals`
- `fps.tlkpproject`
- `fps.tlkpprogram`
- `fps.projectmonth`
- `fps.timecostcalcs`
- `fps.projectmonthcasework`
- `fps.projectmonth2`
- `fps.projectmonth3`
- `fps.projectmonthfinal`
- `fps.tblperiod`
- `fps.tblkperiodmonth`
- `fps.tblkpprofitcentre`
- `fps.profitcentregrade`
- `fps.workgroupgrade`
- `fps.timecodevalid`
- `fps.monthlytime`
- `fps.costcentre`
- `fps.workgroup`
- `fps.monthlyoutput`
- `fps.tlkptestreqmt`
- `fps.period_monthlyoutput`
- `fps.proj_subcontract`
- `fps.period_proj_subcontract`
- `fps.tblwgemployee`
- `fps.period_timecostcalcs`
- `fps.recreatesummaries_log`

Views:
- `fps.qrytotaladditionalcosts`
- `fps.qrytotalanimalcosts`
- `fps.qrytotalstaffcosts`
- `fps.qrytotaltestcosts`
- `fps.qryprojectmonthcw`
- `fps.vpacttblstaff`
- `fps.qryjobmonth_subcontracts`
- `fps.qryjobmonth_time`
- `fps.qryjobmonthmilestone`
- `fps.qryjobmonth_transferstotal`
- `fps.qryjobmonth_invoices`
- `fps.qryjobmonthportfoliosales`
- `fps.qryjobmonth_totprofile`

### 9.4.3 MABArchive Source Objects (fps schema)

Views/tables used as source for loading archive targets:
- `fps.tlkpprogram`
- `fps.tlkpproject`
- `fps.fpsyeartotals`
- `fps.monthlyoutput`
- `fps.monthlytime`
- `fps.proj_invoice`
- `fps.proj_subcontract`
- `fps.projectmonthfinal`
- `fps.tbladditionalcosts`
- `fps.tblanimalreq`
- `fps.tblcontract`
- `fps.tblstaffjob`
- `fps.timecostcalcs`
- `fps.tlkptestreqmt`
- `fps.tbldb_variables`
- `fps.workgroupgrade`
- `fps.profitcentregrade`
- `fps.tblkpprofitcentre`
- `fps.testorproduct`
- `fps.tblwgemployee`
- `fps.tblemployee`
- `fps.workgroup`
- `fps.tblanimals`

### 9.4.4 MABArchive Target Objects (mabarchive schema)

Tables written/populated by MABArchive pipelines:
- `mabarchive.my_tlkpprogram`
- `mabarchive.g_tlkpproject`
- `mabarchive.my_tlkpproject`
- `mabarchive.my_fpsyeartotals`
- `mabarchive.my_monthlyoutput`
- `mabarchive.my_monthlytime`
- `mabarchive.my_proj_invoice`
- `mabarchive.my_proj_subcontract`
- `mabarchive.my_projectmonthfinal`
- `mabarchive.my_tbladditionalcosts`
- `mabarchive.my_tblanimalreq`
- `mabarchive.my_tblcontract`
- `mabarchive.my_tblstaffjob`
- `mabarchive.my_timecostcalcs`
- `mabarchive.my_tlkptestreqmt`
- `mabarchive.tlkpyear`
- `mabarchive.my_workgroupgrade`
- `mabarchive.my_profitcentregrade`
- `mabarchive.my_tblprofitcentre`
- `mabarchive.my_testorproduct`
- `mabarchive.my_staff`
- `mabarchive.my_workgroup`
- `mabarchive.my_tblanimals`
- `mabarchive.my_tlkpproject_all`

## 9.5 Dummy Data Examples (Reader-Friendly)

The examples below are snapshot samples captured from a local BatchJobs foundation database on 2026-06-11.
Values are real sample rows from the provided localhost connection and are included for reader clarity.

### 9.5.1 Manual Trigger Lifecycle Example

`fps.job_master`

| jobid | jobname | frequency | timetolive |
|---|---|---|---|
| 6 | ScheduleJobs | null | 3600 |
| 5 | FECProcess | null | 3600 |
| 4 | MABArchive | null | 3600 |

`fps.job_status`

| statusid | jobid | status |
|---|---|---|
| 16 | 2 | CancelRequested |
| 15 | 4 | Completed |
| 14 | 4 | Cancelled |

`fps.job_queue`

| jobqueueid | jobexecutionid | jobid | statusid | requestedby | startdatetime | enddatetime |
|---|---|---|---|---|---|---|
| 39cf3f26-33bb-43e0-8559-0f4ba3f848e9 | 3713fc69-8f72-4631-8859-ed271c87988e | 2 | 4 | sample-ui@local | 2026-06-11 06:53:58 | 2026-06-11 06:55:31 |
| c9d7e426-caec-4131-bfc6-18ed9746a044 | 9b4c59ff-4e65-40cf-95a5-9cb0952ee6cd | 2 | 4 | sample-ui@local | 2026-06-11 06:34:45 | 2026-06-11 06:35:56 |

`fps.job_queue_log`

| jobqueuelogid | jobqueueid | statusid | performedby | logtime | note |
|---|---|---|---|---|---|
| 110 | 39cf3f26-33bb-43e0-8559-0f4ba3f848e9 | 4 | sample-ui@local | 2026-06-11 06:55:31 | Execution completed |
| 109 | 39cf3f26-33bb-43e0-8559-0f4ba3f848e9 | 2 | sample-ui@local | 2026-06-11 06:53:58 | Execution started |

`fps.job_lock`

| lock_id | job_name | jobqueueid | acquired_at | expires_at | is_active |
|---|---|---|---|---|---|
| (no active rows) |  |  |  |  |  |

`fps.job_cancellation_request`

| jobexecutionid | requested_by | requested_at_utc | status | source | consumed_at_utc |
|---|---|---|---|---|---|
| 421804cb-f6f0-47b3-aad9-d333d356d9e8 | sample-ui@local | 2026-06-04 15:57:06 | Terminalized | pact.api | 2026-06-04 15:57:29 |
| c8296403-cde5-48f6-9299-8e423223005e | sample-ui@local | 2026-06-04 14:43:09 | Pending | pact.api | null |

### 9.5.2 RecreateSummaries Data Shape Example

`fps.tlkpproject`

| parentproject | program | fpsyear | manager |
|---|---|---|---|
| PRJMO | PRG1 | 2026 | null |

`fps.projectmonthfinal`

| project | monthno | fpsyear | totalcost | invoices | cumcost |
|---|---|---|---|---|---|
| PRJMO | 12 | 2026 | 0.00 | 0.00 | null |
| PRJMO | 11 | 2026 | 0.00 | 0.00 | null |

`fps.recreatesummaries_log`

| id | userid | period | datedone | fpsyear |
|---|---|---|---|---|
| 184 | sample-ui@local | 6 | 2026-06-11 12:25:23 | 2026 |
| 183 | sample-ui@local | 6 | 2026-06-11 12:05:50 | 2026 |

### 9.5.3 MABArchive Output Example

`mabarchive.my_tlkpproject`

| year | parentproject | program | customer | projectstatus |
|---|---|---|---|---|
| 1998 | AC1PORT1 | LabT | null | Agreed |
| 1998 | AI TESTS | IT_SURV | CVOG | Agreed |

`mabarchive.my_fpsyeartotals`

| year | parentproject | program | totalcosts | totalincome |
|---|---|---|---|---|
| 1999 | BA1PORT1 | Test_Port | 494568 | 515000.00 |
| 1999 | BA1PORT2 | Test_Port | 53507 | 55000.00 |

## 9.6 Seed Script for `fps.job_master` and `fps.job_status`

Purpose:
- Seed the four known BatchJobs (`MABArchive`, `RecreateSummaries`, `FECProcess`, `YearEnd`) into `fps.job_master`.
- Seed the same four lifecycle statuses (`Pending`, `Running`, `Completed`, `Failed`) for each seeded job in `fps.job_status`.
- Keep script idempotent so it can be safely re-run.

```sql
BEGIN;

-- 1) Seed job catalog rows (idempotent by jobname)
INSERT INTO fps.job_master (jobname, frequency, note, timetolive)
SELECT s.jobname, s.frequency, s.note, s.timetolive
FROM (
  VALUES
    ('MABArchive', NULL::varchar(50), 'Scheduled job; not API-triggerable', 3600),
    ('RecreateSummaries', NULL::varchar(50), 'Manual/API trigger job (PACT API)', 3600),
    ('FECProcess', NULL::varchar(50), 'Manual/API trigger job (FPS API)', 3600),
    ('YearEnd', NULL::varchar(50), 'Manual/API trigger job (FPS API)', 3600)
) AS s(jobname, frequency, note, timetolive)
WHERE NOT EXISTS (
  SELECT 1
  FROM fps.job_master jm
  WHERE jm.jobname = s.jobname
);

-- 2) Seed per-job statuses (idempotent by jobid + status)
INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, st.status
FROM fps.job_master jm
CROSS JOIN (
  VALUES
    ('Pending'),
    ('Running'),
    ('Completed'),
    ('Failed')
) AS st(status)
WHERE jm.jobname IN ('MABArchive', 'RecreateSummaries', 'FECProcess', 'YearEnd')
  AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
    AND js.status = st.status
  );

COMMIT;
```

Verification query:

```sql
SELECT
  jm.jobname,
  COUNT(js.statusid) AS status_count,
  STRING_AGG(js.status, ', ' ORDER BY js.status) AS statuses
FROM fps.job_master jm
LEFT JOIN fps.job_status js ON js.jobid = jm.jobid
WHERE jm.jobname IN ('MABArchive', 'RecreateSummaries', 'FECProcess', 'YearEnd')
GROUP BY jm.jobname
ORDER BY jm.jobname;
```

---

## 10. Implementation Notes (Corrected)

1. API MUST use configured event source (`pact.api` or `fps.api`) in both event publish and API response.
2. API MUST NOT hardcode source in endpoint response.
3. In status mapping, expose:
   - `queueId` from `jobqueueid` (UUID)
   - `jobExecutionId` from `jobexecutionid` (GUID)
4. API MUST publish `parametersJson` in PutEvents detail exactly as provided after validation and map it directly to `BATCH_JOB_PARAMETERS_JSON`. Note: `parametersJson` is intentionally a string because EventBridge maps it directly into a container environment variable. APIs must serialize the object to a JSON string before publishing (e.g., `"{\"month\":\"2026-06\"}"` not `{"month":"2026-06"}` as an object).
5. API SHOULD log `jobExecutionId`, `eventId`, `correlationId`, `Idempotency-Key`, and source for observability.
6. RecreateSummaries local-process flow MAY also set compatibility env vars (`BATCH_RECREATE_SUMMARIES_MONTH`, `BATCH_RECREATE_SUMMARIES_YEAR`) derived from `parametersJson.month`.
7. API SHOULD validate EventBridge source at startup against approved values (`pact.api`, `fps.api`) to prevent contract drift via misconfiguration.

---

## 11. Security and IAM

| Component | Permission | Purpose |
|---|---|---|
| FPS API task role | `events:PutEvents` on approved bus | Publish trigger events |
| PACT API task role | `events:PutEvents` on approved bus | Publish trigger events |
| Worker task role | No `events:PutEvents` required | Worker consumes/executes |
| EventBridge invoke role | `ecs:RunTask`, `iam:PassRole` | Start ECS worker task |

Security requirements:
1. API MUST provide non-empty `requestedBy` for trigger and cancel requests.
2. Worker MUST record `requestedBy` for audit and troubleshooting.
3. Authenticated identity mapping in caller APIs SHOULD be implemented when available but is out of scope for this integration handoff.

---

## 12. Acceptance Criteria

1. Trigger accepted returns `202` with `jobExecutionId` and `acceptedAtUtc`.
2. If idempotency is implemented, duplicate trigger with same idempotency key returns same acceptance response.
3. If idempotency is implemented, duplicate key with different payload returns `409 IdempotencyConflict`.
4. Event publish failures return `503 DispatchFailed` and do not create false active state.
5. Status endpoint is deterministic by `jobExecutionId` with no latest-by-job fallback for user-facing UI requests.
6. Startup watchdog transitions to `StartFailedTimeout` after SLA when no DB row appears.
7. UI polling stops only on terminal states.
8. Cancel endpoint is deterministic and idempotent.
9. Source values are restricted to `fps.api`/`pact.api` and reflected consistently.
10. Observability fields are logged and searchable.

---

## 13. Implementation Considerations for Next Iteration

### 13.1 Idempotency-Key Header Support (Current Gap)

**Current behavior:**
- PACT/FPS APIs accept every trigger POST as new; no request fingerprinting or replay cache.
- Immediate browser refresh or network retry results in duplicate EventBridge publish.
- Lock table prevents concurrent same-job execution but does not prevent duplicate event emission.

**Recommended next step:**
- Implement HTTP `Idempotency-Key` header (RFC 9110-style semantics) on trigger endpoints.
- Store request fingerprint (hash of jobName + parameters + requestedBy) keyed by Idempotency-Key in trigger-attempt store (Memory in non-prod, Redis in prod).
- Return 202 + same `jobExecutionId` + `acceptedAtUtc` on replay within TTL (e.g., 60 minutes).
- Rationale: eliminates duplicate EventBridge invocations on browser double-submit; lock remains as last-line safety.

### 13.2 Partial Indexes for Production Query Performance

**Current schema:**
- Single partial unique index exists: `uq_job_lock_job_name_active` on `fps.job_lock (job_name) WHERE is_active = TRUE`.
- This is essential and should not be removed.

**Recommended production enhancements (assess by query-plan analysis):**
1. Lock cleanup + active-lock read optimization:
   - `idx_job_lock_job_name_expires_at_active` on `fps.job_lock (job_name, expires_at) WHERE is_active = TRUE`.
   - Rationale: aligns with TryAcquireLockAsync cleanup predicate and active-lock scan patterns.

2. Cancellation table operational scans (if introduced):
   - Only if operational dashboards scan cancellation requests by status (e.g., show pending cancellations).
   - Partial: `idx_job_cancel_status_pending` on `fps.job_cancellation_request (requested_at_utc DESC) WHERE status = 'Pending'`.
   - Not required for current point-lookups by jobExecutionId.

3. Job queue active execution scans (if introduced):
   - Only if dashboards frequently query running/pending rows across all jobs.
   - Partial: `idx_job_queue_active_runs` on `fps.job_queue (jobid, requested_at_utc DESC) WHERE statusid IN (select statusid from fps.job_status where status IN ('Running','Pending','Retry'))`.
   - Not required for correlated-by-jobExecutionId lookups, which hit unique index.

**Assessment rule:** Request a query-plan analysis after 2-3 months of production load before adding speculative indexes.

### 13.3 Design Alternative: Store JobExecutionId in Database Instead of Local Cache

**Current design (hybrid):**
- `jobExecutionId` is generated at API acceptance time and immediately cached (Memory/Redis).
- Cached entry persists for 60 minutes.
- Database row (`fps.job_queue`) is created asynchronously via worker startup.
- Status endpoint reads cache first (startup watchdog projection); falls back to DB (source of truth).

**Alternative: Full database-first design**

**Trade-offs:**

| Dimension | Cache-First (Current) | Database-First (Alternative) |
|---|---|---|
| **Startup observability gap** | Minimal (0–30s dev, 0–600s prod). Cache bridges the gap. | Depends on job creation latency. If worker row creation takes >2s, UI sees null/not-found for 2s. |
| **Store coupling** | Two stores: Memory/Redis + DB. Must sync on completion. | One store: DB. Simpler conceptual model. |
| **Refresh behavior** | Browser refresh: cache hit (millisecond). | Browser refresh: DB query (millisecond but DB latency-dependent). |
| **Idempotency window** | 60-min cache TTL allows Idempotency-Key replay. | Unique constraint on `jobexecutionid` prevents duplicate rows; but repeated EventBridge publishes still occur (lock handles runtime). |
| **Operational cost** | Requires cache layer (Redis for prod). Memory for non-prod. | No cache layer overhead. Simpler infra. |
| **Lock interaction** | Lock in same table (fps.job_lock). Independent stores. | Lock and execution in same schema. Single transactional boundary potential. |
| **Stale state cleanup** | Cache eviction by TTL (automatic). Lock cleanup by expire scan + status check. | DB only: must run explicit cleanup jobs for stale locks/requests. |
| **Correlated polling reliability** | High: cache hit guarantees correlation within TTL. Fallback to latest-by-job if cache miss. | Medium: DB row might not exist yet. Must implement timeout → fallback to latest-by-job. |

**Database-first implications:**

1. **Schema change required:**
   - Add `accepted_at_utc TIMESTAMPTZ` column to `fps.job_queue`.
   - Move DB row creation from worker startup to API acceptance (before EventBridge publish).
   - Worker must use `jobexecutionid` from env var to find and update existing row (not create).

2. **API-to-Worker contract change:**
   - API creates execution row immediately (status = Pending).
   - Worker receives pre-populated `BATCH_JOBQUEUE_ID` to locate and transition row.
   - Benefit: no missing-row gap; worker cannot lose its own execution.
   - Risk: API must handle DB write errors during publish; if publish succeeds but row creation fails, orphan event occurs.

3. **Startup observability:**
   - Remove cache-based `TriggerAccepted` projection; move to DB-backed status query.
   - Status endpoint checks `job_queue` row with accepted_at_utc populated.
   - Introduces slight query latency (ms-level) but deterministic.

4. **Idempotency:**
   - Unique index on `jobexecutionid` prevents duplicate DB rows.
   - But API still publishes duplicate EventBridge events on retry; lock prevents same job from running concurrently.
   - Idempotency-Key header still recommended for true request replay, but DB row dedup is a bonus.

5. **Lock table:**
   - Can remain independent or merge into `job_queue` with `lock_token` / `lock_expires_at` columns.
   - Merging simplifies lifecycle (one DELETE for cleanup) but bloats row size and couples concerns.
   - Recommend keeping separate for operational clarity.

6. **Cleanup and recovery:**
   - Must add background job to clean up stale/expired locks from `fps.job_lock`.
   - Must add background job to clean up orphaned rows in `fps.job_queue` (e.g., rows with Pending status and accepted_at_utc older than 10 minutes).
   - Current cache-based design does not require these cleanup jobs.

**Recommendation:**
- Current cache-first design is correct for immediate production needs: low latency, clear observability, acceptable operational complexity.
- Migrate to database-first only if (a) cache layer becomes bottleneck, (b) you require single-store simplicity for compliance, or (c) you want to eliminate Idempotency-Key as a separate concern.
- If you choose database-first, introduce it as a feature branch with parallel validation against cache-first results before GA.

---

## 14. Team-Specific Guidance

### 14.1 For PACT API and FPS API Teams

**Responsibilities:**
- Implement all endpoints listed in Section 5.1 (trigger, status, can-run, cancel).
- Validate request payloads and return correct HTTP status codes per Section 5.6.
- Publish events to EventBridge with required fields from Section 6.
- Implement idempotency per Section 5.4 (target-state; currently a SHOULD).
- Log correlation IDs and observability fields for debugging.

**Acceptance checks before staging:**
1. Health endpoint responds on `/health`.
2. Trigger endpoint returns `202` with `jobExecutionId` and `acceptedAtUtc`.
3. Status endpoint accepts `jobExecutionId` query param and returns deterministic state or `400` if missing.
4. Cancel endpoint is deterministic by `jobExecutionId`.
5. EventBridge `PutEvents` API is called with correct source and detail-type.
6. All non-2xx responses include error envelope per Section 5.6.

### 14.2 For BatchJobs Worker Team

**Responsibilities:**
- Read all `BATCH_*` env vars from container overrides (EventBridge input transformer).
- Validate `BATCH_JOB_NAME` against allowed jobs before executing.
- Acquire lock via `BatchLockRepository.TryAcquireLockAsync()` before execution.
- Create execution row in `fps.job_queue` with status `Pending` then `Running`.
- Parse `BATCH_REQUESTED_AT_UTC` if present; persist when valid. If absent, follow scheduled/manual fallback policy (see Section 6). If invalid, store as null and log a warning.
- Persist `requestedBy` for all executions (audit trail).
- Write terminal status and error context on failure.
- Poll `fps.job_cancellation_request` at cancellation checkpoints and exit gracefully if `Consumed`.
- Release lock after execution (or on error).

**Acceptance checks before staging:**
1. Execution rows are created with correct `jobexecutionid` from env var.
2. `requested_at_utc` is parsed and persisted when provided; null otherwise.
3. Lock acquire respects unique constraint on `job_name` with `is_active = TRUE`.
4. Lock is released on success and on error paths.
5. Terminal states are written with correct `statusid` and `enddatetime`.
6. Cancel requests are polled and consumed correctly.
7. Worker container exits with status code 0 on success, non-zero on failure.

### 14.3 For Lead Architect

**Decision points:**
- Cache-first vs database-first approach (see Section 13.3; recommendation is cache-first for GA).
- Idempotency-Key implementation priority (Section 13.1; high priority for next iteration).
- Extend startup watchdog to FPS API (consistency and observability).
- Multi-region deployment implications (EventBridge rule targets, Redis replication if used).

**Review checklist:**
- Event sourcing and EventBridge rule configuration aligned with approved source list.
- Lock and cancellation request semantics provide adequate safety for concurrent invocations.
- Error handling and retry logic is explicit and documented.
- Startup latency targets are realistic given ECS cold start and DB roundtrip times.

### 14.4 For DBA and Infrastructure Team

**Responsibilities:**
- Create/verify `fps.job_master`, `fps.job_status`, `fps.job_queue`, `fps.job_queue_log`, `fps.job_lock`, `fps.job_cancellation_request` tables and indexes.
- Maintain unique partial index on `fps.job_lock (job_name) WHERE is_active = TRUE`.
- Monitor and tune indexes after 2–3 months of production load (see Section 13.2).
- Implement stale-lock cleanup job if advised by query-plan analysis.
- Configure EventBridge Scheduler targets and container environment variable mapping.
- Monitor ECS task success rate and EventBridge DLQ metrics.

**Performance expectations:**
- Status query by `jobExecutionId` should complete in <10ms (unique index).
- Lock acquire/release should be <20ms (atomic INSERT/DELETE with partial index).
- Cancellation polling should be <20ms per point-lookup (indexed by `jobexecutionid`).

**Monitoring queries:**
- Active lock count: `SELECT COUNT(*) FROM fps.job_lock WHERE is_active = TRUE;`
- Pending cancellation requests: `SELECT COUNT(*) FROM fps.job_cancellation_request WHERE status = 'Pending';`
- Running executions: `SELECT COUNT(DISTINCT jobid) FROM fps.job_queue WHERE statusid IN (SELECT statusid FROM fps.job_status WHERE status IN ('Running','Pending','Retry'));`
- Stale locks (not matching queue status): `SELECT lock_id, job_name FROM fps.job_lock WHERE is_active = TRUE AND lock_id NOT IN (SELECT jobqueueid FROM fps.job_queue WHERE statusid IN (SELECT statusid FROM fps.job_status WHERE status IN ('Running','Pending','Retry')));`

---

## 15. Known Gaps and Risks

**Current implementation gaps (target-state items):**

| Gap | Current | Target | Impact | Owner |
|---|---|---|---|---|
| Idempotency-Key header support | Not implemented | RFC 9110 semantics | Duplicate EventBridge publishes on browser retry | PACT/FPS API teams |
| Startup watchdog in FPS API | No watchdog projection | Extend from PACT | Opaque startup state for FPS-triggered jobs | FPS API team |
| Durable cancellation request table | `fps.job_cancellation_request` exists but not fully integrated in all paths | Full CRUD integration and worker polling in all critical paths | Cancel requests may not propagate if job already started | Worker team / API teams |
| Cleanup jobs for stale locks/requests | Manual ad-hoc queries | Automated background job | Stale data accumulation in fps.job_lock / fps.job_cancellation_request | DevOps / DBA |
| Production index tuning | Single partial unique index on job_lock | Add conditional indexes (see Section 13.2) | Query slowness under high load | DBA (assess after 2–3 months) |

**Known risks and mitigations:**

| Risk | Scenario | Mitigation |
|---|---|---|
| EventBridge publish succeeds but worker never invokes | Network/ECS infrastructure failure | Manual DLQ review; alarms on failed invocations |
| API publish fails but trigger accepted message already sent to client | Transient AWS error | Client retries; idempotency-key prevents duplicates (target) |
| Worker crashes after lock acquire but before releasing | Process kill/OOM | Stale lock self-healing in GetActiveLockAsync detects and releases |
| DB row created but cache entry expires before UI checks status | Long cache miss (>60 min idle) | Deterministic status by jobExecutionId falls back to DB; not a functional loss, just slower |
| Cancellation request created but worker starts before polling | Rare timing window | Worker polls at startup and at each checkpoint; acceptable race window |

---

## 16. Sign-off

This contract is proposed for review by PACT API, FPS API, BatchJobs Worker, and DevOps teams. Sign below to confirm agreement with sections 2–13 and team-specific guidance in Section 14.

| Role | Name | Date | Approval / Comments |
|---|---|---|---|
| PACT API Lead |  |  |  |
| FPS API Lead |  |  |  |
| BatchJobs Worker Lead |  |  |  |
| DevOps / CCoE |  |  |  |
| Lead Architect / Solutions |  |  |  |
| Database Administrator |  |  |  |

**How to use this document:**

1. **For discussion:** Print or share Section 0 (Executive Summary) + Sections 2, 5, 6, 8 during team meetings. Use Section 14 (Team-Specific Guidance) to walk through each team's responsibilities.

2. **For implementation:** Developers reference Sections 5 (API), 6 (EventBridge), and 9 (DB) as normative requirements. Sections 4 and 4.1 provide worked examples.

3. **For sign-off:** Each team lead reviews Section 14 applicable to their domain, then signs Section 16.

4. **For operations:** Section 14.4 (DBA) provides monitoring queries and performance expectations. Section 15 (Known Gaps) highlights items to watch.

5. **For next iteration:** Section 13 (Next Iteration Considerations) captures architectural decisions to revisit after GA.
