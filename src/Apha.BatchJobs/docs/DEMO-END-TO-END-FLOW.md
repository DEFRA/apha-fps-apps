---
title: "BatchJobs + PACT API - End-to-End Demo Flow"
version: "1.1"
audience: "Demo Participants, PACT Team, BatchJobs Team"
---

# BatchJobs & PACT API - Complete Demo Flow

## Overview

This document describes the **end-to-end flow** that will be demonstrated, showing:
1. How the **Sample UI** (consumer) triggers a job via **PACT API**
2. How **PACT API** manages startup watchdog and returns real-time status
3. How **BatchJobs** persists all 7 state transitions to the database
4. How the **Sample UI** polls and renders states according to the formal contract

---

## Demo Setup Verification Checklist

Before starting the demo, verify:

- [ ] PostgreSQL running on `localhost:5432`
- [ ] Database `batch_jobs_foundation_db_cloud` exists
- [ ] Migration `106_add_missing_job_statuses.sql` has been executed (adds Pending, Retry, Skipped to fps.job_status)
- [ ] PACT API running on `http://localhost:5189`
- [ ] Sample UI running on `http://localhost:5003`
- [ ] Network connectivity between all components
- [ ] Formal state machine contract reviewed by participants

---

## Scenario 1: Happy Path (Normal Completion)

**Objective**: Demonstrate complete successful execution with all state transitions visible.

### Timeline

```
T+0s    | User clicks "Trigger RecreateSummaries" in Sample UI
        ↓
        POST /api/v1/batch-jobs/trigger
        Body: { "jobName": "RecreateSummaries", "requestedBy": "demo@local" }
        ↓
        PACT API Response: HTTP 202 Accepted
        {
                                        "accepted": true,
                                        "source": "pact.api",
                                        "jobExecutionId": "a1b2c3d4e5f647a89b0c1d2e3f4a5b6c",
          "eventId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
                                        "status": "TriggerAccepted",
          "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
                                        "jobName": "RecreateSummaries",
                                        "message": "Trigger accepted for dispatch."
        }
        ↓
T+0-2s  | Sample UI enters "Submitting" state
        | UI shows: "Submitting trigger request..."
        | Timeline: "Trigger submitted for RecreateSummaries by demo@local"

T+1s    | Sample UI begins polling
        | GET /api/v1/batch-jobs/RecreateSummaries/status?jobExecutionId=a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c
        ↓
        PACT API sees acceptedAtUtc but NO execution record in DB yet
        ↓
        Watchdog COMPUTES: "TriggerAccepted" (not yet deadline)
        ↓
        PACT Response: HTTP 200
        {
          "jobName": "RecreateSummaries",
          "isRunning": false,
                                        "sourceOfTruth": "StartupWatchdog",
                                        "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
          "lastExecution": null,
          "startupWatchdog": {
            "projectedState": "TriggerAccepted",
                                                "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
                                                "startupDeadlineUtc": "2026-06-03T14:30:30.123Z"
          }
        }
        ↓
T+1-2s  | Sample UI shows: "TriggerAccepted" / "Queued"
        | Timeline: "Status: TriggerAccepted (watchdog, awaiting Pending)"

T+2-3s  | [BATCH JOBS WORKER STARTS]
        | Writes first DB record: fps.job_queue with statusid="Pending"
        | 
        | INSERT INTO fps.job_queue (..., statusid=1, ...)
        | where statusid=1 is "Pending" from fps.job_status

T+3s    | Sample UI polls again (every 2-5s during startup phase)
        ↓
        PACT queries fps.job_queue:
        ```
        SELECT js.status, jq.* FROM fps.job_queue jq
        JOIN fps.job_status js ON jq.statusid = js.statusid
        WHERE jq.jobexecutionid = 'a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c'
        LIMIT 1;
        ```
        Result: status='Pending', state is NOW in DB
        ↓
        Watchdog NO LONGER NEEDED (execution found in DB)
        ↓
        PACT Response: HTTP 200
        {
          "jobName": "RecreateSummaries",
          "isRunning": false,
          "lastExecution": {
            "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
            "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
                                                "status": "Pending",
                                                "businessState": "Running",
                                                "startedAt": "2026-06-03T14:30:02.000Z",
                                                "completedAt": null,
            "errorMessage": null
          },
          "startupWatchdog": null
        }
        ↓
T+3-4s  | Sample UI shows: "Pending"
        | Timeline: "Status: Pending (queued at 14:30:02)"
        | **Polling interval: Still 2-5s (startup phase)**

T+5s    | [BATCH JOBS WORKER BEGINS EXECUTION]
        | Transitions: UPDATE fps.job_queue SET statusid=2 WHERE jobqueueid=...
        | statusid=2 is "Running"

T+6s    | Sample UI polls, receives "Running" from DB
        | PACT Response shows lastExecution.status="Running"
        ↓
T+6-7s  | Sample UI shows: "Running"
        | Timeline: "Status: Running (started 14:30:05)"
        | **Polling interval: Changes to 15-30s (execution phase, less load)**

T+45s   | [BATCH JOBS WORKER COMPLETES]
        | Transitions: UPDATE fps.job_queue SET statusid=3 WHERE jobqueueid=...
        | statusid=3 is "Completed"
        | Also sets: endDateTime = NOW(), updated_at = NOW()

T+47s   | Sample UI polls (next scheduled poll at T+21s, T+36s, T+51s, ...)
        | PACT Response shows lastExecution.status="Completed"
        ↓
T+47-48s| Sample UI shows: "Completed" (green checkmark)
        | Timeline: "Status: Completed successfully"
        | **Polling STOPS (terminal state reached)**
        | UI shows: "Re-trigger RecreateSummaries" button becomes enabled

DEMO NOTES:
- Total time: ~50 seconds from trigger to completion
- User sees progressive state updates: Submitting → Accepted → Pending → Running → Completed
- Each state is clearly labeled and time-stamped in the timeline
- All transitions come from either PACT watchdog (transient) or database (persistent)
```

---

## Scenario 2: Watchdog Timeout (Worker Fails to Start)

**Objective**: Demonstrate watchdog mechanism detecting startup failure.

### Timeline

```
T+0s    | User triggers job (same as Scenario 1, T+0-3s)
        | PACT accepts, Sample UI shows "TriggerAccepted"

T+1-3s  | Sample UI polls; PACT still sees NO DB record
        | Watchdog projects "TriggerAccepted" (still within SLA)

T+30s   | [NO BATCH JOBS RECORD EVER CREATED]
        | Startup SLA deadline (30 seconds in dev/local) has PASSED

T+31s  | Sample UI polls again
        ↓
        PACT queries: acceptedAtUtc + 30s < now() ✓ TRUE
        ↓
        Watchdog computes: "StartFailedTimeout"
        ↓
        PACT Response: HTTP 200
        {
          "startupWatchdog": {
            "projectedState": "StartFailedTimeout",
                                                "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
                                                "startupDeadlineUtc": "2026-06-03T14:30:30.123Z",
                                                "evaluatedAtUtc": "2026-06-03T14:30:31.500Z"
          }
        }
        ↓
T+31-32s | Sample UI shows: "StartFailedTimeout" (red)
        | Timeline: "Status: StartFailedTimeout (watchdog)"
        | **Polling STOPS (terminal state: watchdog timeout)**
        | UI shows: "Retry RecreateSummaries" button becomes enabled

KEY INSIGHT:
- Worker never wrote to database
- Only PACT watchdog detected the failure
- No "Pending" state ever persisted
- Timeout proof: acceptedAtUtc=14:30:00 + 30s dev/local deadline exceeded (600s in production)
```

---

## Scenario 3: Retry Flow (Job Fails Then Auto-Retries)

**Objective**: Demonstrate retry mechanism with state transitions Failed → Retry → Pending.

### Timeline

```
T+0s    | User triggers job (same happy path startup: Accepted → Pending)

T+5s    | Execution becomes "Running"

T+40s   | [BATCH JOBS DETECTS ERROR]
        | Exception in job logic: "Database query timeout"
        | Error is RETRYABLE (transient, not permanent)
        ↓
        | BatchJobs writes: UPDATE SET statusid=4 (Failed)
        | Also stores: errorMessage="Database query timeout after 30s"

T+41s   | Sample UI polls
        | Receives lastExecution.status="Failed" with error message
        ↓
T+41-42s | Sample UI shows: "Failed" (red)
        | Timeline: "Status: Failed - Database query timeout after 30s"
        | **Polling STOPS - appears terminal**
        | UI shows: "Retry RecreateSummaries" button

T+43s   | [RETRY SCHEDULER DETECTS FAILED STATE]
        | BatchJobs retry service reads fps.job_queue where statusid=4
        | Evaluates: Is error retryable? YES
        | Transitions: UPDATE SET statusid=6 (Retry)
        |
        | This signals: "Job failed, but retry is scheduled"

T+44s   | Sample UI polls
        | Receives lastExecution.status="Retry"
        ↓
T+44-45s | Sample UI shows: "Retry" (orange spinner)
        | Timeline: "Status: Retry (retry scheduler will move to Pending)"
        | **Polling CONTINUES (not terminal; retry in progress)**

T+45s   | [RETRY SCHEDULER AUTO-RESETS]
        | Transitions: UPDATE SET statusid=1 (Pending)
        | This means: job is queued again for retry execution

T+46s   | Sample UI polls
        | Receives lastExecution.status="Pending" (back to queued state)
        ↓
T+46-47s | Sample UI shows: "Pending"
        | Timeline: "Status: Pending (queued at 14:35:46)"

T+47s   | [BATCH JOBS WORKER PICKS UP RETRY]
        | Transitions: UPDATE SET statusid=2 (Running)

T+48-90s | Execution runs successfully this time
        | Transitions: UPDATE SET statusid=3 (Completed)

T+91s   | Sample UI shows: "Completed" (green)

FULL TIMELINE:
Running → Failed → Retry → Pending → Running → Completed

KEY INSIGHTS:
- Retry is a PERSISTENT DB state, not a transient projection
- Retry is intermediate (not terminal); polling continues
- Automatic transition: Failed → Retry → Pending happens server-side
- No user action required; automatic recovery
- UI shows user what's happening: "Retrying..."
```

---

## Scenario 4: Concurrent Trigger Rejection (Lock Collision)

**Objective**: Demonstrate HTTP 409 conflict when job already running.

### Timeline

```
T+0s    | First user triggers "RecreateSummaries"
        | PACT accepts; job starts running

T+15s   | Second user (or same user, rapid click) triggers again
        | POST /api/v1/batch-jobs/trigger
        ↓
        PACT checks: Is job already running? 
        Query: SELECT ... FROM fps.job_queue WHERE jobname='RecreateSummaries' 
               AND currentStatus='Running'
        Result: YES, found active execution
        ↓
        PACT Response: HTTP 409 CONFLICT
        {
          "error": "Job 'RecreateSummaries' is already running",
          "currentExecution": {
            "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
            "startedAt": "2026-06-03T14:30:05.000Z"
          }
        }
        ↓
T+15-16s | Second UI/user sees HTTP 409
        | Timeline: "Trigger rejected: Job already running (started at 14:30:05)"
        | UI shows: Button disabled, error message displayed

KEY INSIGHT:
- Single-run policy enforced at trigger time (not state machine)
- HTTP 409 is proper REST response for conflict
- Second user informed of ongoing execution details
- No "Skipped" state persisted (rejection happened before queue entry)
```

---

## Formal Contract Validation Checklist

During demo, verify each point:

### Trigger Endpoint (/api/v1/batch-jobs/trigger)
- [ ] Returns HTTP 202 Accepted on success
- [ ] Response includes `eventId` (correlates to jobExecutionId)
- [ ] Response includes `acceptedAtUtc` (used by watchdog)
- [ ] Returns HTTP 409 Conflict if job already running (with details)
- [ ] Returns HTTP 400 Bad Request if job name invalid
- [ ] Request body requires `jobName` and `requestedBy`

### Status Endpoint (/api/v1/batch-jobs/{jobName}/status)
- [ ] Returns HTTP 200 with full PACT contract response
- [ ] Response includes both `lastExecution` (DB) and `startupWatchdog` (computed)
- [ ] `lastExecution` includes `status` (raw DB state) and `businessState` (UI projection)
- [ ] `status` values match 7 states: Pending, Running, Completed, Failed, Cancelled, Retry, Skipped
- [ ] Watchdog computes: TriggerAccepted, WorkerProcessStarted, StartFailedTimeout, WorkerProcessExited
- [ ] Deterministic tracking is used with `jobExecutionId` query param

### Polling Strategy
- [ ] Sample UI polls every 2-5s during startup (watchdog phase)
- [ ] Sample UI polls every 15-30s during Running state
- [ ] Sample UI stops polling when terminal state reached
- [ ] Terminal states: Completed, Failed, Cancelled, StartFailedTimeout, Skipped, (Retry is NOT terminal)

### Database State Persistence
- [ ] All state changes persisted to `fps.job_queue` table
- [ ] `statusid` foreign key references `fps.job_status.statusid`
- [ ] `updated_at` timestamp updated on each state transition
- [ ] All 7 states appear in demo: Pending, Running, Completed, Failed, Cancelled, Retry, Skipped

### Sample UI User Experience
- [ ] Clear state display with color coding
- [ ] Timeline log shows all state transitions with timestamps
- [ ] Raw PACT API response visible (JSON panel)
- [ ] Button state synced with execution state (disabled during polling, enabled at terminal)
- [ ] Error messages displayed when applicable

---

## Demo Script (Presenter Notes)

### Opening (30 seconds)

> "We're going to demonstrate the BatchJobs + PACT API formal contract. Here's what you'll see:
> 
> - Left side: Sample UI (the consumer polling for status)
> - Right side: Real PACT API responses (the contract in action)
> - Bottom: Timeline of all state transitions
> 
> This demo proves that all 7 database states (Pending, Running, Completed, Failed, Cancelled, Retry, Skipped) are properly managed, and transient watchdog states (TriggerAccepted, StartFailedTimeout) provide real-time feedback."

### Happy Path Demo (60 seconds)

1. **Click "Trigger RecreateSummaries"**
   > "Sending trigger to PACT API..."

2. **Show state transitions**
   > "Watch the timeline on the left. Notice we transition through:
   > - Submitting (initial request)
   > - TriggerAccepted (API watchdog, waiting for worker visibility)
   > - Pending (worker wrote to DB, queued)
   > - Running (worker started execution)
   > - Completed (success!)"

3. **Point to raw API response**
   > "The right panel shows the actual PACT API response. Notice:
        > - `lastExecution.status = 'Running'` (from database)
   > - `startupWatchdog = null` (no longer needed; execution visible in DB)"

### Scenario Showcase (20 seconds each)

4. **Show "Retry" scenario**
   > "Click 'Show Scenario' and select 'Retry'. This shows what happens when a job fails but can retry. Note the state transitions: Running → Failed → Retry → Pending → (would resume Running)."

5. **Show "StartFailedTimeout" scenario**
        > "Select 'StartFailedTimeout'. This is when the worker never starts. PACT watchdog detects it after the startup SLA (30 seconds in dev/local; 600 seconds in production) and declares failure without ever seeing a DB record."

### Closing (30 seconds)

> "All states, transitions, and API responses are formally defined in the contract. PACT API team will implement this exactly; BatchJobs team owns the state persistence. Any questions about the flow?"

---

## Troubleshooting During Demo

| Issue | Cause | Fix |
|-------|-------|-----|
| Sample UI stuck in "Submitting" | PACT API not responding | Check localhost:5189 is running |
| Watchdog timeout never fires | SLA too long or time not passing | Use scenario preview instead |
| `lastExecution.status` field not in response | API not updated to formal contract | Verify PACT API code matches contract |
| States missing from dropdown | Sample UI cache not refreshed | Hard refresh (Ctrl+Shift+R) |
| Database doesn't show new states | Migration not applied | Run 106_add_missing_job_statuses.sql |

---

## Sign-Off

**Demo Prepared By**: [BatchJobs Team]  
**Contract Reviewed By**: [PACT API Lead]  
**Date**: 2026-06-08  
**Contract Version**: 1.1
