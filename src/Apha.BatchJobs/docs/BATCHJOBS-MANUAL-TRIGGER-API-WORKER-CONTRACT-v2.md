# BatchJobs Manual Trigger API-Worker Contract (v2)

## 1. Document Control

- Audience: PACT API Team, FPS API Team, BatchJobs Worker Team, DevOps
- Purpose: Define the API, event, state, database, and implementation contract for manually triggered BatchJobs
- Runtime path: UI -> PACT/FPS API -> Amazon EventBridge Rule -> ECS Fargate BatchJobs Worker -> BatchJobs DB
- Primary correlation key: `jobExecutionId` (GUID), returned to UI and persisted as `fps.job_queue.jobexecutionid`
- Event detail type: `BatchJobTriggerRequested`
- Allowed event sources: `fps.api`, `pact.api`
- Status: Contract draft for team review and sign-off

This contract is intentionally scoped to manual/API-triggered execution. Scheduled setup is out of scope.

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
| PACT API / FPS API | PACT/FPS team | Validate request, generate `jobExecutionId`, derive `requestedBy` from authenticated identity, publish event, expose status/can-run/cancel endpoints |
| EventBridge | DevOps / Platform | Match event pattern and invoke ECS task with required container environment variables |
| BatchJobs Worker | BatchJobs team | Read `BATCH_*` env vars, enforce lock, create/update execution rows, execute job, write status/log |
| BatchJobs DB | BatchJobs team | Authoritative persistent execution state |

Non-goals:
- EventBridge Scheduler configuration
- UI styling details
- internal SQL-to-.NET mapping implementation details
- full operations runbook

---

## 3. Identity and Trust Model (Normative)

1. API is the identity authority for manual triggers.
2. Worker is not an identity authority and does not resolve caller identity.
3. `requestedBy` MUST be populated by API from authenticated principal claims (human or service identity).
4. Client-provided `requestedBy` MUST NOT be treated as authoritative.
5. API MUST publish events only with approved source values (`pact.api` or `fps.api`).
6. Worker MUST trust `requestedBy` only when event source is approved and the event arrives through the approved bus/rule path.

---

## 4. Manual Trigger Flow

1. User triggers job from UI.
2. API validates route policy and request payload.
3. API derives `requestedBy` from authenticated identity.
4. API applies idempotency policy.
5. API generates `jobExecutionId` (GUID) for new accepted trigger.
6. API publishes `BatchJobTriggerRequested` event.
7. EventBridge rule invokes ECS task with container overrides.
8. Worker reads env vars and begins orchestration.
9. Worker writes `Pending` then `Running` and terminal states to DB.
10. UI polls status endpoint by `jobExecutionId` until terminal.

---

## 5. API Contract

## 5.1 Endpoints

| Method | Endpoint | Required | Purpose |
|---|---|---|---|
| GET | `/health` | Optional | Liveness check |
| GET | `/api/v1/batch-jobs/catalog` | Yes | List routable jobs and route policy |
| GET | `/api/v1/batch-jobs/{jobName}/can-run` | Recommended | Advisory pre-check |
| POST | `/api/v1/batch-jobs/trigger` | Yes | Accept trigger and publish event |
| GET | `/api/v1/batch-jobs/{jobName}/status?jobExecutionId={guid}` | Yes | Deterministic status by correlation key |
| POST | `/api/v1/batch-jobs/{jobName}/{jobExecutionId}/cancel` | Recommended | Deterministic idempotent cancellation |

## 5.2 Trigger Request

`POST /api/v1/batch-jobs/trigger`

Headers:
- `Idempotency-Key` (required): unique key per client intent (UUID recommended)

Body:

```json
{
  "jobName": "RecreateSummaries",
  "parametersJson": "{\"month\":\"2026-06\"}"
}
```

Rules:
1. `jobName` is required.
2. `parametersJson` is optional JSON string and MUST contain a valid JSON object that satisfies job-specific schema.
3. `requestedBy` is not accepted as authoritative request input.
4. API derives `requestedBy` from authenticated identity.

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

1. API MUST require `Idempotency-Key` on trigger requests.
2. API MUST persist dedupe records for at least 24 hours.
3. Duplicate trigger with same key and same effective request payload MUST return the original acceptance response (`202`) and same `jobExecutionId`.
4. Duplicate key with different payload MUST return `409 IdempotencyConflict`.

## 5.5 Cancellation Rules

Endpoint: `POST /api/v1/batch-jobs/{jobName}/{jobExecutionId}/cancel`

Rules:
1. Cancellation target is deterministic by `jobExecutionId`.
2. Repeated cancel requests are idempotent.
3. If execution is already terminal, return success with terminal state and no-op message.
4. Worker remains final authority for cooperative cancellation points.

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

#### 1.1.1.1 Event pattern

```json
{
  "source": ["fps.api", "pact.api"],
  "detail-type": ["BatchJobTriggerRequested"]
}
```

#### 1.1.1.2 Example event payload

```json
{
  "source": "pact.api",
  "detail-type": "BatchJobTriggerRequested",
  "detail": {
    "jobExecutionId": "7f9d2f2e8d1b4f7a9d256d6e8a9f3c12",
    "jobName": "RecreateSummaries",
    "runMode": "Manual",
    "requestedBy": "user.name@defra.gov.uk",
    "requestedAtUtc": "2026-06-09T13:41:27Z",
    "parametersJson": "{\"month\":\"2026-06\"}"
  }
}
```

#### 1.1.1.3 Input transformer mapping

| Event detail field | Input path | Worker env var |
|---|---|---|
| `jobExecutionId` | `$.detail.jobExecutionId` | `BATCH_JOB_EXECUTION_ID` |
| `jobName` | `$.detail.jobName` | `BATCH_JOB_NAME` |
| `runMode` | `$.detail.runMode` | `BATCH_RUN_MODE` |
| `requestedBy` | `$.detail.requestedBy` | `BATCH_REQUESTED_BY` |
| `requestedAtUtc` | `$.detail.requestedAtUtc` | `BATCH_REQUESTED_AT_UTC` |
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

#### 1.1.1.4 Input template / container override example

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

### 1.3.2.5 BATCH_JOB_PARAMETERS_JSON requirements

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
1. API MUST evaluate startup timeout at 120 seconds from `acceptedAtUtc` unless overridden by config.
2. Before timeout and before DB row appears, status MAY report projected state (`TriggerAccepted` or `WorkerProcessStarted`).
3. After timeout with no DB row, status MUST report `StartFailedTimeout`.

Status response should include:
- `sourceOfTruth`: `StartupWatchdog` or `BatchJobs`
- `correlatedJobExecutionId`
- `lastExecution` (when DB row exists)
- `startupWatchdog` object when projection state is active

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
- `fps.job_queue.jobqueueid`: queue row ID (numeric in current schema)

APIs MUST NOT assume `jobqueueid` is GUID.

## 9.3 Recommended Status Query

```sql
SELECT
    jq.jobqueueid,
    jq.jobexecutionid,
    jm.jobname,
    js.status,
    jq.requestedby,
    jq.startdatetime,
    jq.enddatetime,
    jq.errormessage,
    jq.created_at,
    jq.updated_at
FROM fps.job_queue jq
JOIN fps.job_master jm ON jm.jobid = jq.jobid
JOIN fps.job_status js ON js.statusid = jq.statusid
WHERE jq.jobexecutionid = @jobExecutionId
  AND jm.jobname = @jobName
ORDER BY jq.created_at DESC
LIMIT 1;
```

---

## 10. Implementation Notes (Corrected)

1. Use configured event source (`pact.api` or `fps.api`) in both event publish and API response.
2. Do not hardcode source in endpoint response.
3. In status mapping, expose:
   - `queueId` from `jobqueueid` (numeric)
   - `jobExecutionId` from `jobexecutionid` (GUID)
4. Publish `parametersJson` in PutEvents detail exactly as provided after validation; map it directly to `BATCH_JOB_PARAMETERS_JSON`.
5. Log `jobExecutionId`, `eventId`, `correlationId`, `Idempotency-Key`, and source for observability.

---

## 11. Security and IAM

| Component | Permission | Purpose |
|---|---|---|
| FPS API task role | `events:PutEvents` on approved bus | Publish trigger events |
| PACT API task role | `events:PutEvents` on approved bus | Publish trigger events |
| Worker task role | No `events:PutEvents` required | Worker consumes/executes |
| EventBridge invoke role | `ecs:RunTask`, `iam:PassRole` | Start ECS worker task |

Security requirements:
1. API MUST derive `requestedBy` from authenticated identity in cloud runtime.
2. API MUST reject unauthenticated trigger requests.
3. Worker MUST record `requestedBy` for audit and troubleshooting.

---

## 12. Acceptance Criteria

1. Trigger accepted returns `202` with `jobExecutionId` and `acceptedAtUtc`.
2. Duplicate trigger with same idempotency key returns same acceptance response.
3. Duplicate key with different payload returns `409 IdempotencyConflict`.
4. Event publish failures return `503 DispatchFailed` and do not create false active state.
5. Status endpoint is deterministic by `jobExecutionId`.
6. Startup watchdog transitions to `StartFailedTimeout` after SLA when no DB row appears.
7. UI polling stops only on terminal states.
8. Cancel endpoint is deterministic and idempotent.
9. Source values are restricted to `fps.api`/`pact.api` and reflected consistently.
10. Observability fields are logged and searchable.

---

## 13. Sign-off Checklist

| Checklist Item | PACT API | FPS API | Worker | DevOps |
|---|---|---|---|---|
| Trigger/status/can-run/cancel endpoints implemented | Yes | Yes | N/A | N/A |
| Event published with required fields | Yes | Yes | Consumes | Rule maps |
| `requestedBy` derived from auth identity | Yes | Yes | Records only | N/A |
| Idempotency implemented for trigger | Yes | Yes | N/A | N/A |
| UI polls by `jobExecutionId` | Yes | Yes | Persists same ID | N/A |
| IAM permissions in place | Required | Required | Not required | Configure/approve |
| EventBridge invoke role runs ECS task | N/A | N/A | N/A | Required |

Sign-off:

| Role | Name | Date | Approval / Comments |
|---|---|---|---|
| PACT API Lead |  |  |  |
| FPS API Lead |  |  |  |
| BatchJobs Worker Lead |  |  |  |
| DevOps / CCoE |  |  |  |
| Architecture Review |  |  |  |
