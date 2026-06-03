---
title: "PACT API Quick Reference - BatchJobs State Machine"
version: "1.0"
audience: "PACT API Development Team"
---

# PACT API Quick Reference Card

## 7 Persistent DB States (Always Use These as Source of Truth)

```
Pending      → Job queued, waiting for worker pickup
Running      → Job actively executing
Completed    → ✅ Job finished successfully (TERMINAL)
Failed       → ❌ Job error detected (may trigger Retry)
Cancelled    → ⊘ Job stopped by user/system (TERMINAL)
Retry        → ⟳ Job failed; retry scheduled (transitions to Pending automatically)
Skipped      → ⊘ Job rejected at trigger time (TERMINAL)
```

## 3 Transient Watchdog States (PACT Computation Only)

```
TriggerAccepted         → Initial phase: trigger accepted, awaiting DB record
WorkerProcessStarted    → Local mode: process spawned, awaiting DB visibility
StartFailedTimeout      → SLA deadline exceeded with no DB record (TERMINAL watchdog state)
```

---

## Trigger Endpoint Quickstart

```http
POST /api/v1/batch-jobs/trigger
Content-Type: application/json

{
  "jobName": "RecreateSummaries",
  "requestedBy": "user@example.com"
}
```

### Success Response
```json
HTTP 202 Accepted
{
  "eventId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
  "jobName": "RecreateSummaries",
  "message": "Batch job trigger accepted"
}
```

### Conflict Response (Job Already Running)
```json
HTTP 409 Conflict
{
  "error": "Job 'RecreateSummaries' is already running",
  "currentExecution": {
    "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
    "startedAt": "2026-06-03T14:00:00.000Z"
  }
}
```

---

## Status Endpoint Quickstart

```http
GET /api/batch-jobs/RecreateSummaries/status
```

### Response (Execution in DB)
```json
HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  
  "lastExecution": {
    "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
    "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
    "currentState": "Running",                      ← Database state
    "stateTimestamp": "2026-06-03T14:30:45.123Z",
    "startDateTime": "2026-06-03T14:30:00.000Z",
    "endDateTime": null,
    "requestedBy": "user@example.com",
    "errorMessage": null
  },
  
  "startupWatchdog": null                           ← No watchdog (execution found)
}
```

### Response (Startup Watchdog Phase)
```json
HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  
  "lastExecution": null,                            ← No DB record yet
  
  "startupWatchdog": {
    "isWatchdogActive": true,
    "triggerAcceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "projectedState": "TriggerAccepted",            ← Watchdog projection
    "evaluatedAtUtc": "2026-06-03T14:30:15.000Z",
    "startupSlaSeconds": 180,
    "deliveryExhaustionConfirmed": false
  }
}
```

---

## Watchdog Algorithm (Pseudo-Code)

```csharp
if (execution == null && acceptedAtUtc.HasValue)
{
    var now = DateTime.UtcNow;
    var sla = environment.IsProduction() ? 600 : 180;  // seconds
    var deadline = acceptedAtUtc.Value.AddSeconds(sla);
    
    var projectedState = now > deadline 
        ? "StartFailedTimeout" 
        : "TriggerAccepted";
    
    return new Watchdog
    {
        isWatchdogActive = true,
        projectedState,
        startupDeadlineUtc = deadline,
        evaluatedAtUtc = now
    };
}
return null;  // Execution found in DB; no watchdog needed
```

---

## Polling Strategy for Clients

| Phase | Condition | Poll Interval | Stop When |
|-------|-----------|---------------|-----------|
| **Startup** | lastExecution == null, watchdog active | 2-5 seconds | Pending appears in DB |
| **Active** | currentState == "Running" | 15-30 seconds | Terminal state reached |
| **Terminal** | Completed, Failed, Cancelled, Skipped, or Timeout | STOP | N/A |

### Client Decision Logic (Pseudo-Code)

```javascript
function getNextPollInterval(response) {
    // If execution found in DB
    if (response.lastExecution) {
        const state = response.lastExecution.currentState;
        
        // Terminal states: stop polling
        if (['Completed', 'Failed', 'Cancelled', 'Skipped'].includes(state)) {
            return null;  // STOP
        }
        
        // Retry is NOT terminal; keep polling
        if (state === 'Retry') {
            return 2000;  // Fast poll to see transition
        }
        
        // Running: slow poll to reduce load
        if (state === 'Running') {
            return 15000;
        }
        
        // Pending: still startup phase
        return 2000;
    }
    
    // If watchdog active (no DB record yet)
    if (response.startupWatchdog?.isWatchdogActive) {
        
        // Timeout: stop polling
        if (response.startupWatchdog.projectedState === 'StartFailedTimeout') {
            return null;  // STOP
        }
        
        // Still waiting: fast poll
        return 2000 + Math.random() * 3000;
    }
    
    return null;  // Should never reach here
}
```

---

## Responsibility Boundaries

### PACT API Team ✅
- Accept trigger requests (HTTP 202)
- Check for concurrent running (HTTP 409 if locked)
- Query BatchJobs DB for execution status
- Compute watchdog projections (Pending → Completed during startup window)
- Serve status responses with both DB state + watchdog

### BatchJobs Team ✅
- Persist all 7 states to `fps.job_queue`
- Transition states: Pending → Running → Completed/Failed/Cancelled
- Auto-retry: Failed → Retry → Pending
- Enforce single-run (mutual exclusion per job name)
- Set `updated_at` on all state transitions
- Publish events (optional future enhancement)

---

## Database Schema (Read-Only from PACT)

```sql
-- Reference: all allowed states per job
SELECT * FROM fps.job_status
WHERE jobid = (SELECT jobid FROM fps.job_master WHERE jobname = 'RecreateSummaries');
-- Returns: Pending, Running, Completed, Failed, Cancelled, Retry, Skipped

-- Execution: current instance
SELECT js.status, jq.* FROM fps.job_queue jq
JOIN fps.job_status js ON jq.statusid = js.statusid
WHERE jq.jobexecutionid = 'a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c'
LIMIT 1;
-- Returns: One row with current state, timestamps, error message
```

---

## Error Codes

| Code | Meaning | Action |
|------|---------|--------|
| 202 | Trigger accepted | Proceed to polling |
| 200 | Status query successful | Check currentState field |
| 400 | Invalid job name | Validate job exists in system |
| 409 | Job already running | Show conflict message; retry later |
| 500 | Server error | Retry with exponential backoff |
| Network timeout | Connection failed | Retry with exponential backoff |

---

## Common Patterns

### Pattern 1: Detect Terminal State
```javascript
const isTerminal = ['Completed', 'Failed', 'Cancelled', 'Skipped']
    .includes(response.lastExecution?.currentState);
```

### Pattern 2: Detect Watchdog Timeout
```javascript
const isTimeout = response.startupWatchdog?.projectedState === 'StartFailedTimeout';
```

### Pattern 3: Job is Retrying
```javascript
const isRetrying = response.lastExecution?.currentState === 'Retry';
// Keep polling; retry scheduler will transition to Pending automatically
```

### Pattern 4: Still Waiting for Worker Visibility
```javascript
const isAwaitingWorker = response.lastExecution === null 
    && response.startupWatchdog?.isWatchdogActive === true
    && response.startupWatchdog?.projectedState !== 'StartFailedTimeout';
```

---

## Testing Checklist

- [ ] Trigger endpoint returns 202 with eventId + acceptedAtUtc
- [ ] Status endpoint returns lastExecution with currentState from DB
- [ ] Watchdog projects TriggerAccepted when DB record not yet visible
- [ ] Watchdog projects StartFailedTimeout when deadline exceeded
- [ ] HTTP 409 returned when second trigger sent during Running state
- [ ] All 7 state transitions visible in polling sequence
- [ ] Retry transitions: Failed → Retry → Pending happen automatically
- [ ] Polling intervals respected: 2-5s startup, 15-30s running
- [ ] Raw JSON response matches OpenAPI schema

---

**Contract Version**: 1.0  
**Last Updated**: 2026-06-03  
**Questions?** Consult `BATCHJOBS-STATE-MACHINE-CONTRACT.md` for details.
