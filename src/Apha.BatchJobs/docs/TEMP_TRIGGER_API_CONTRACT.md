# Temporary Batch Trigger API Contract

Purpose: define a stable, temporary API contract inside Apha.BatchJobs so UI and FPS integration can proceed end-to-end now, then migrate with minimal changes later.

Base route: `/api/batch-jobs`

## 1) Trigger Job

### Endpoint

- Method: `POST`
- Path: `/api/batch-jobs/{jobName}/trigger`

### Behaviour

- Validates `jobName`.
- Checks if job is already running.
- If runnable, accepts request immediately and starts execution asynchronously.
- UI should poll status endpoint after receiving accepted response.

### Success Response (Accepted)

- Status: `202 Accepted`

```json
{
  "accepted": true,
  "operationId": "b99fcf0c7cf541cc9139e8665fd26a53",
  "acceptedAt": "2026-04-21T11:10:45.123Z",
  "jobName": "HealthCheck",
  "message": "Job accepted for execution. Poll status endpoint for progress."
}
```

### Conflict Response (Already Running)

- Status: `409 Conflict`

```json
{
  "accepted": false,
  "reason": "Job is already running",
  "jobName": "HealthCheck",
  "runId": "c81dc5e56a694f37adf81300f2aa55bb",
  "acquiredAt": "2026-04-21T11:06:10.122Z",
  "expiresAt": "2026-04-21T12:06:10.122Z"
}
```

### Not Found Response (Unknown Job)

- Status: `404 Not Found`

```json
{
  "error": "Job 'UnknownJob' is not registered.",
  "jobName": "UnknownJob"
}
```

## 2) Can-Run Check

### Endpoint

- Method: `GET`
- Path: `/api/batch-jobs/{jobName}/can-run`

### Runnable Response

- Status: `200 OK`

```json
{
  "canRun": true
}
```

### Not Runnable Response

- Status: `200 OK`

```json
{
  "canRun": false,
  "reason": "Job is already running",
  "runId": "c81dc5e56a694f37adf81300f2aa55bb",
  "acquiredAt": "2026-04-21T11:06:10.122Z",
  "expiresAt": "2026-04-21T12:06:10.122Z"
}
```

## 3) Job Status (Single)

### Endpoint

- Method: `GET`
- Path: `/api/batch-jobs/{jobName}/status`

### Response

- Status: `200 OK`

```json
{
  "jobName": "HealthCheck",
  "isRunning": false,
  "activeLock": null,
  "lastExecution": {
    "runId": "c81dc5e56a694f37adf81300f2aa55bb",
    "status": "Completed",
    "startedAt": "2026-04-21T11:06:10.122Z",
    "completedAt": "2026-04-21T11:06:42.311Z"
  }
}
```

## 4) Job Status (All)

### Endpoint

- Method: `GET`
- Path: `/api/batch-jobs`

### Response

- Status: `200 OK`
- Body: array of status objects in same shape as single-job status response.

## 5) UI Integration Flow

1. UI optionally calls `GET /api/batch-jobs/{jobName}/can-run` to decide button state.
2. On click, UI calls `POST /api/batch-jobs/{jobName}/trigger`.
3. If `202 Accepted`, UI starts polling `GET /api/batch-jobs/{jobName}/status`.
4. UI stops polling when `isRunning = false` and `lastExecution.status` is terminal (`Completed`, `Failed`, `Cancelled`, `Skipped`).

## 6) Migration Notes (Temporary -> FPS Backend)

- Keep route paths and response fields stable where possible to avoid UI rewrite.
- Replace only trigger implementation behind API layer (temporary in-process execution -> FPS container/task start).
- Preserve semantics:
  - `202` means accepted asynchronously.
  - `409` means cannot start due to active run.
  - status endpoints remain source of truth for progress.
