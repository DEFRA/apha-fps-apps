---
title: "BatchJobs Execution State Machine Contract (Formal Specification)"
version: "1.0"
date: "2026-06-03"
audience: "PACT API Team, BatchJobs Team"
status: "APPROVED"
---

# BatchJobs Execution State Machine Contract

## Executive Summary

This document defines the **formal contract** between **BatchJobs Team** (execution layer) and **PACT API Team** (trigger/status layer) regarding job execution states, transitions, and API response formats.

**Key Principle**: BatchJobs is the authoritative source of truth for persistent job execution state. PACT API provides transient polling artifacts during the startup window.

---

## 1. State Definitions

### 1.1 Persistent States (Source of Truth: `fps.job_queue.statusid`)

These 7 states are stored in the database and represent the actual execution lifecycle:

| State | ID | Purpose | Duration | Next Valid States |
|-------|-----|---------|----------|-------------------|
| **Pending** | 1 | Job queued, awaiting worker assignment | Seconds to minutes | Running |
| **Running** | 2 | Job actively executing on worker | Seconds to hours | Completed, Failed, Cancelled |
| **Completed** | 3 | Job finished successfully | Terminal | None (query historical) |
| **Failed** | 4 | Job encountered error; automatic retry may follow | 1-30 seconds | Retry (auto) or Terminal |
| **Cancelled** | 5 | Job terminated by user/system before completion | Terminal | None (query historical) |
| **Retry** | 6 | Job failure detected; retry scheduled | Seconds to minutes | Pending (auto) |
| **Skipped** | 7 | Job rejected at trigger time (lock, policy) | Immediate | None (rejection event, not stored in queue) |

### 1.2 Transient States (Source: PACT Status API Computation)

These states are **NOT stored in the database**. They are computed by PACT API at request time to provide better UX feedback during the startup window:

| State | API Projection Logic | When to Use | Display to User |
|-------|----------------------|-------------|-----------------|
| **TriggerAccepted** | `acceptedAtUtc` recorded; no DB execution record yet | During watchdog phase (0-3 min) | Yes (as "Pending/Queued") |
| **WorkerProcessStarted** | Local dispatcher confirms process spawn (local mode only) | During startup (local dev/test only) | Optionally (debug mode) |
| **StartFailedTimeout** | `acceptedAtUtc + startupSlaSeconds < now()` AND no DB record | After SLA deadline exceeded | Yes (as "Startup Timeout") |

---

## 2. State Transition Rules (Formal State Machine)

### 2.1 Valid Transitions

```
CLIENT TRIGGER
      ↓
   TriggerAccepted (API projection)
      ↓
   [WATCHDOG PHASE: 0-180s]
   ├─→ Skipped (if lock/policy rejection)
   ├─→ StartFailedTimeout (if SLA deadline exceeded)
   └─→ Pending (when worker writes first DB record)
      ↓
   Running (worker actively executing)
      ├─→ Completed (on success)
      ├─→ Failed (on error)
      │    └─→ Retry (auto-scheduled)
      │        └─→ Pending (auto-reset by retry scheduler)
      └─→ Cancelled (user/system cancellation)
```

### 2.2 Invalid Transitions (Guaranteed Rejected by State Machine)

- `Completed` → `Running` ❌ Terminal state
- `Cancelled` → `Running` ❌ Terminal state
- `Completed` → `Failed` ❌ No state regression
- `Running` → `Pending` ❌ Cannot revert to queued
- `Skipped` → `Pending` ❌ Skipped is rejection event, not queued
- Any state → `Skipped` ❌ Skipped only at trigger time

---

## 3. API Contract (PACT Status Endpoint)

### 3.1 Endpoint Specification

```
GET /api/batch-jobs/{jobName}/status
```

**Query Parameters:**
- `jobName` (string, required): Name of the batch job (e.g., "RecreateSummaries")

**Response: HTTP 200 OK**

```json
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  
  "lastExecution": {
    "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
    "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
    "currentState": "Running",
    "stateTimestamp": "2026-06-03T14:30:45.123Z",
    "startDateTime": "2026-06-03T14:30:00.000Z",
    "endDateTime": null,
    "requestedBy": "user@example.com",
    "errorMessage": null
  },
  
  "startupWatchdog": {
    "isWatchdogActive": true,
    "triggerAcceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "startupDeadlineUtc": "2026-06-03T14:35:00.000Z",
    "projectedState": "TriggerAccepted",
    "evaluatedAtUtc": "2026-06-03T14:30:30.000Z",
    "startupSlaSeconds": 300,
    "deliveryExhaustionConfirmed": false
  }
}
```

### 3.2 Response Field Semantics

| Field | Responsibility | Notes |
|-------|-----------------|-------|
| `jobName` | PACT | Echo of request parameter |
| `isRunning` | PACT | `true` if `currentState == "Running"` |
| `sourceOfTruth` | PACT | Always "BatchJobs" (for future multi-source support) |
| `correlatedJobExecutionId` | PACT | From last trigger acceptance or DB query |
| `lastExecution.*` | PACT querying BatchJobs | From `fps.job_queue` + `fps.job_status` join |
| `lastExecution.currentState` | **BatchJobs (via DB)** | Primary source of truth; 7 possible values |
| `startupWatchdog.*` | **PACT computation** | Transient projections; set only if watchdog active |
| `startupWatchdog.projectedState` | PACT | One of: `TriggerAccepted`, `WorkerProcessStarted`, `StartFailedTimeout` |

### 3.3 Watchdog Algorithm (PACT Responsibility)

```csharp
if (execution == null && acceptedAtUtc.HasValue)
{
    var now = DateTime.UtcNow;
    var environment = hostEnvironment.EnvironmentName;
    var startupSlaSeconds = environment.IsProduction() ? 600 : 180; // 10 min prod, 3 min dev
    var startupDeadlineUtc = acceptedAtUtc.Value.AddSeconds(startupSlaSeconds);
    
    var isWatchdogActive = true;
    var projectedState = now > startupDeadlineUtc 
        ? "StartFailedTimeout" 
        : "TriggerAccepted";
    
    return new StartupWatchdog 
    { 
        isWatchdogActive, 
        projectedState, 
        evaluatedAtUtc = now,
        startupDeadlineUtc,
        startupSlaSeconds
    };
}
```

### 3.4 Error Responses

**HTTP 404 Not Found** (when job name doesn't exist)
```json
{ "error": "Job 'InvalidName' not found in system" }
```

**HTTP 500 Internal Server Error** (DB connection failure)
```json
{ "error": "Unable to query execution status. Please retry." }
```

---

## 4. Trigger Endpoint Contract

### 4.1 Request Specification

```
POST /api/v1/batch-jobs/trigger
Content-Type: application/json

{
  "jobName": "RecreateSummaries",
  "requestedBy": "user@example.com"
}
```

### 4.2 Success Response: HTTP 202 Accepted

```json
{
  "eventId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
  "jobName": "RecreateSummaries",
  "message": "Batch job trigger accepted"
}
```

### 4.3 Rejection Responses

**HTTP 409 Conflict** (job already running - lock collision)
```json
{
  "error": "Job 'RecreateSummaries' is already running",
  "currentExecution": {
    "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
    "startedAt": "2026-06-03T14:00:00.000Z"
  }
}
```

**HTTP 400 Bad Request** (invalid job name)
```json
{
  "error": "Job 'InvalidName' not found"
}
```

---

## 5. Client Polling Contract (Sample UI / Consumer)

### 5.1 Recommended Polling Strategy

**Phase 1: Startup/Watchdog Phase (0-180 seconds)**
- Poll interval: 2-5 seconds
- Stop condition: `currentState` enters one of {Running, Completed, Failed, Cancelled, Skipped} OR watchdog timeout
- Display: Show `projectedState` from watchdog OR `currentState` if available

**Phase 2: Active Execution (Running)**
- Poll interval: 15-30 seconds (longer to reduce load)
- Stop condition: `currentState` is terminal (Completed, Failed, Cancelled)

**Phase 3: Retry Detection**
- If `currentState == "Retry"`: automatically repeat Phase 1
- Inform user: "Job failed; automatic retry scheduled"

### 5.2 State Display Mapping for UI

```javascript
const stateDisplayMapping = {
  // Transient/watchdog states (show temporarily)
  'TriggerAccepted': { label: 'Queued', color: 'blue', icon: 'hourglass' },
  'WorkerProcessStarted': { label: 'Starting', color: 'blue', icon: 'spinner' },
  'StartFailedTimeout': { label: 'Startup Failed', color: 'red', icon: 'alert' },
  
  // Persistent DB states
  'Pending': { label: 'Pending', color: 'blue', icon: 'clock' },
  'Running': { label: 'Running', color: 'blue', icon: 'spinner' },
  'Completed': { label: 'Completed', color: 'green', icon: 'checkmark' },
  'Failed': { label: 'Failed', color: 'red', icon: 'error' },
  'Cancelled': { label: 'Cancelled', color: 'gray', icon: 'stop' },
  'Retry': { label: 'Retrying', color: 'orange', icon: 'refresh' },
  'Skipped': { label: 'Skipped', color: 'gray', icon: 'skip' }
};
```

---

## 6. Database Schema Alignment

### 6.1 Reference Table: `fps.job_status`

```sql
CREATE TABLE IF NOT EXISTS fps.job_status (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_status_jobid
        FOREIGN KEY (jobid) REFERENCES fps.job_master(jobid) ON DELETE CASCADE,
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);
```

**Seeded Values for Each Job**: Pending, Running, Completed, Failed, Cancelled, Retry, Skipped

### 6.2 Execution Queue Table: `fps.job_queue`

```sql
CREATE TABLE IF NOT EXISTS fps.job_queue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobexecutionid UUID NOT NULL,
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,
    requestedby VARCHAR(100) NOT NULL,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_queue_jobid
        FOREIGN KEY (jobid) REFERENCES fps.job_master(jobid),
    CONSTRAINT fk_job_queue_statusid
        FOREIGN KEY (statusid) REFERENCES fps.job_status(statusid)
);
```

---

## 7. Responsibilities Matrix

### BatchJobs Team (Execution Layer)

- ✅ Record initial `Pending` state when worker starts
- ✅ Transition `Running` when job begins processing
- ✅ Transition to `Completed`, `Failed`, or `Cancelled` at terminal state
- ✅ Auto-transition `Failed` → `Retry` if retryable
- ✅ Auto-transition `Retry` → `Pending` when retry scheduled
- ✅ Persist all state changes with `updated_at` timestamp
- ✅ Publish state change events (optional future integration)
- ✅ Guarantee single-run (mutual exclusion) per job name

### PACT API Team (Trigger/Status Layer)

- ✅ Accept trigger requests; return 202 with `eventId` + `acceptedAtUtc`
- ✅ Persist trigger acceptance metadata
- ✅ Compute watchdog projections during startup window
- ✅ Query `fps.job_queue` to get current `lastExecution` state
- ✅ Join `fps.job_status` to get state name from `statusid`
- ✅ Return both DB state AND watchdog projection in status response
- ✅ Enforce SLA timeout for startup window
- ✅ Return HTTP 409 for concurrent trigger attempts (lock conflict)
- ✅ Reject invalid job names with HTTP 400

---

## 8. Testing Scenarios (Validation Checklist)

### Scenario 1: Normal Completion Flow
```
✓ POST /trigger → 202 Accepted (eventId, acceptedAtUtc)
✓ GET /status → TriggerAccepted (watchdog)
✓ [0-3s] Worker writes Pending state
✓ GET /status → Pending (from DB)
✓ Worker transitions Running
✓ GET /status → Running (from DB)
✓ Worker completes successfully
✓ GET /status → Completed (from DB)
```

### Scenario 2: Watchdog Timeout (No Worker)
```
✓ POST /trigger → 202 Accepted
✓ GET /status → TriggerAccepted (0s)
✓ [repeat until deadline]
✓ GET /status @ 181s → StartFailedTimeout (watchdog)
✓ No state persisted in DB (execution never started)
```

### Scenario 3: Concurrent Trigger Rejection
```
✓ POST /trigger (1st) → 202 Accepted
✓ POST /trigger (2nd, immediate) → 409 Conflict
✓ Error response contains active execution details
✓ GET /status → Running (current execution)
```

### Scenario 4: Retry Flow
```
✓ Job reaches Running state
✓ Worker detects retryable error
✓ BatchJobs sets Failed → Retry (auto)
✓ Retry scheduler detects Retry state
✓ Auto-transitions Retry → Pending
✓ Worker restarts job
✓ GET /status shows Pending (polling sees retry in progress)
```

### Scenario 5: Manual Cancellation
```
✓ Job in Running state
✓ User/system sends cancel signal
✓ BatchJobs transitions Running → Cancelled
✓ GET /status → Cancelled (terminal)
```

---

## 9. Deployment Checklist

### BatchJobs Team Pre-Deployment
- [ ] `fps.job_status` seeded with all 7 states per job
- [ ] State transition logic tested for all valid transitions
- [ ] Retry scheduler tested (Failed → Retry → Pending)
- [ ] Single-run/lock mechanism verified
- [ ] `created_at` and `updated_at` timestamps populated on all transitions
- [ ] Error handling for DB constraints
- [ ] Logging of all state transitions for audit trail

### PACT API Team Pre-Deployment
- [ ] Watchdog algorithm implemented per Section 3.3
- [ ] Startup SLA thresholds configured (180s dev, 600s prod)
- [ ] Status endpoint returns both DB state + watchdog projection
- [ ] Lock conflict detection (409 response) implemented
- [ ] Polling clients can handle all 7 state values + 3 transient states
- [ ] Error responses formatted per Section 4.3
- [ ] Load testing for rapid polling (2-5s intervals)

---

## 10. Future Enhancements (Out of Scope for v1.0)

- [ ] Event streaming (BatchJobs publishes state change events to event bus)
- [ ] Delivery exhaustion tracking (for external trigger retries)
- [ ] Job execution logs/audit trail per execution
- [ ] State-specific metadata (e.g., retry count, error stack trace)
- [ ] Multi-tenant job isolation
- [ ] Scheduled job support (vs. ad-hoc triggering)

---

## 11. Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| BatchJobs Lead | [TBD] | 2026-06-03 | ___________ |
| PACT API Lead | [TBD] | 2026-06-03 | ___________ |
| Architecture Review | [TBD] | 2026-06-03 | ___________ |

---

**Document Version**: 1.0  
**Last Updated**: 2026-06-03  
**Next Review**: After first production deployment
