---
title: "PACT API - BatchJobs Integration Handoff"
version: "1.0"
audience: "PACT API Development Team"
date: "2026-06-03"
status: "PRODUCTION-READY"
---

# PACT API ↔ BatchJobs Integration Handoff

## Executive Summary

This document officially hands off the **formal API contract** and **state machine specification** from the **BatchJobs team** to the **PACT API team** for implementation.

**Key Deliverables:**
1. ✅ Formal State Machine Contract (signed specification)
2. ✅ OpenAPI 3.0 Specification (machine-readable)
3. ✅ Working Sample UI (demonstrates end-to-end flow)
4. ✅ Demo Scenarios (4 validated use cases)
5. ✅ Database Schema (all 7 states in production DB)
6. ✅ Quick Reference Card (for rapid development)

**Status**: READY FOR PACT IMPLEMENTATION

---

## Document Index

| Document | Purpose | Audience | Read Time |
|----------|---------|----------|-----------|
| [BATCHJOBS-STATE-MACHINE-CONTRACT.md](BATCHJOBS-STATE-MACHINE-CONTRACT.md) | **MASTER SPECIFICATION** - Legal contract between teams | Contract signers, architects | 30 min |
| [PACT-API-OPENAPI.yaml](PACT-API-OPENAPI.yaml) | **TECHNICAL SPEC** - API schemas in OpenAPI format | PACT developers | 15 min |
| [PACT-API-QUICK-REFERENCE.md](PACT-API-QUICK-REFERENCE.md) | **DEVELOPER GUIDE** - Quick lookup during coding | PACT developers | 5-10 min |
| [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md) | **VALIDATION PROOF** - 4 demo scenarios with timelines | Testers, QA, demo attendees | 20 min |
| [PACT-BATCHJOBS-HANDOFF-EVENTGRID.md](PACT-BATCHJOBS-HANDOFF-EVENTGRID.md) | **ARCHITECTURE DETAILS** - Watchdog, state transitions, retry | System architects | 25 min |

**Start Here**: Read **BATCHJOBS-STATE-MACHINE-CONTRACT.md** (Section 1 only if short on time)

---

## Current PACT API Endpoints (Implemented)

The current public implementation exposes 5 endpoints in PACT API.

| Endpoint | Purpose | Typical Status |
|----------|---------|----------------|
| `GET /health` | Service liveness check | `200` |
| `GET /api/v1/batch-jobs/catalog` | Job routing catalog for UI/client policy checks | `200` |
| `POST /api/v1/batch-jobs/trigger` | Accept and dispatch a trigger request | `202`, `409` |
| `GET /api/batch-jobs/{jobName}/can-run` | Lock-aware guardrail check before trigger | `200` |
| `GET /api/batch-jobs/{jobName}/status?jobExecutionId=<guid>` | Correlated run status + watchdog projection | `200` |

Internal/local-only break-glass route (not part of public integration contract):
- `POST /internal/local/batch-jobs/{jobName}/break-glass/release-lock`
- Available only in `Development` / `Local` environments.

### 1. GET /health
What it does:
- Returns basic health metadata for PACT API process.

Request example:
```bash
curl "http://localhost:5189/health"
```

Response example (`200 OK`):
```json
{
  "status": "healthy",
  "service": "pact.api",
  "timestamp": "2026-06-04T10:22:11.341Z"
}
```

### 2. GET /api/v1/batch-jobs/catalog
What it does:
- Returns known jobs and whether they can be triggered from PACT API.

Request example:
```bash
curl "http://localhost:5189/api/v1/batch-jobs/catalog"
```

Response example (`200 OK`):
```json
{
  "api": "pact.api",
  "jobs": [
    {
      "jobName": "RecreateSummaries",
      "description": "Mapped to PACT API",
      "routeKind": "PactApi",
      "canTriggerFromThisApi": true
    },
    {
      "jobName": "MABArchive",
      "description": "Scheduled job only; year is derived internally from execution date",
      "routeKind": "ScheduledOnly",
      "canTriggerFromThisApi": false
    }
  ]
}
```

### 3. POST /api/v1/batch-jobs/trigger
What it does:
- Validates job route policy.
- Generates immutable `jobExecutionId`.
- Dispatches trigger to configured dispatcher (EventBridge or local process).
- Stores trigger-attempt metadata used by status watchdog.

Request example:
```bash
curl -X POST "http://localhost:5189/api/v1/batch-jobs/trigger" \
  -H "Content-Type: application/json" \
  -d '{"jobName":"RecreateSummaries","requestedBy":"demo@local"}'
```

Accepted response example (`202 Accepted`):
```json
{
  "accepted": true,
  "source": "pact.api",
  "jobName": "RecreateSummaries",
  "jobExecutionId": "7fdf872e2b5841fe887b6bdbb8ea596a",
  "eventId": "localproc-11080",
  "workerPid": 11080,
  "workerProcessLaunched": true,
  "status": "WorkerProcessStarted",
  "acceptedAtUtc": "2026-06-04T10:22:11.341Z",
  "message": "Trigger accepted and local worker process launched. Attach debugger to workerPid."
}
```

Route-policy rejection example (`409 Conflict`):
```json
{
  "accepted": false,
  "source": "pact.api",
  "jobName": "MABArchive",
  "reason": "Job 'MABArchive' is scheduled-only and cannot be triggered by API."
}
```

### 4. GET /api/batch-jobs/{jobName}/can-run
What it does:
- Checks active distributed lock plus active execution state.
- Returns a simple UI guardrail decision.

Request example:
```bash
curl "http://localhost:5189/api/batch-jobs/RecreateSummaries/can-run"
```

Response example (`200 OK`):
```json
{
  "jobName": "RecreateSummaries",
  "canRun": false,
  "reason": "Job is already running (active distributed lock).",
  "activeLock": {
    "jobQueueId": "5b0f0eaf-f763-4de0-aedf-a28a8c93fbec",
    "acquiredAt": "2026-06-04T10:22:15.111Z",
    "expiresAt": "2026-06-04T10:42:15.111Z",
    "isActive": true
  },
  "sourceOfTruth": "BatchJobs"
}
```

### 5. GET /api/batch-jobs/{jobName}/status?jobExecutionId=<guid>
What it does:
- Returns DB-backed `lastExecution` when available.
- If no execution record exists yet, computes `startupWatchdog` projection from trigger-attempt store.
- Provides `sourceOfTruth` so UI can remain render-only.

Request example:
```bash
curl "http://localhost:5189/api/batch-jobs/RecreateSummaries/status?jobExecutionId=7fdf872e2b5841fe887b6bdbb8ea596a"
```

Response example while waiting for execution row (`200 OK`):
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "7fdf872e2b5841fe887b6bdbb8ea596a",
  "lastExecution": null,
  "startupWatchdog": {
    "projectedState": "WorkerProcessStarted",
    "acceptedAtUtc": "2026-06-04T10:22:11.341Z",
    "startupDeadlineUtc": "2026-06-04T10:22:41.341Z",
    "evaluatedAtUtc": "2026-06-04T10:22:16.211Z",
    "startupSlaSeconds": 30,
    "deliveryExhaustionConfirmed": false,
    "deliveryExhaustionOwner": "IntegrationTransportReconciler",
    "eventId": "localproc-11080",
    "triggerStatus": "WorkerProcessStarted",
    "workerExitCode": null,
    "triggerStore": "PactInMemoryCache"
  }
}
```

Response example when execution exists (`200 OK`):
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "7fdf872e-2b58-41fe-887b-6bdbb8ea596a",
  "lastExecution": {
    "executionId": 12345,
    "jobName": "RecreateSummaries",
    "jobExecutionId": "7fdf872e-2b58-41fe-887b-6bdbb8ea596a",
    "status": "Running",
    "startedAt": "2026-06-04T10:22:20.000Z",
    "completedAt": null,
    "durationSeconds": null,
    "recordsProcessed": 1250,
    "recordsFailed": 0,
    "errorMessage": null
  },
  "startupWatchdog": null
}
```

---

## The 7 Job States (Always Use These)

```
┌─────────────────────────────────────┐
│ DATABASE STATE MACHINE (Persistent) │
└─────────────────────────────────────┘

1. Pending      ← Job queued, awaiting worker pickup
   └─→ Running

2. Running      ← Job actively executing
   ├─→ Completed    (success)
   ├─→ Failed       (error)
   └─→ Cancelled    (stopped by user)

3. Completed    ← Job finished successfully (TERMINAL)

4. Failed       ← Job error detected (may auto-retry)
   └─→ Retry

5. Retry        ← Retry scheduled by retry scheduler (NOT TERMINAL)
   └─→ Pending

6. Cancelled    ← Job stopped (TERMINAL)

7. Skipped      ← Job rejected at trigger time (TERMINAL)


┌──────────────────────────────────────────────┐
│ WATCHDOG STATES (Computed, Not Persisted)    │
└──────────────────────────────────────────────┘

Computed during startup window (0-30s non-prod, 0-600s prod):
- TriggerAccepted           ← Initial: awaiting DB visibility
- WorkerProcessStarted      ← Local mode: process spawned
- StartFailedTimeout        ← Deadline exceeded, no DB record

All transitions are defined in BATCHJOBS-STATE-MACHINE-CONTRACT.md Section 2
```

---

## Watchdog And Trigger Store (Critical)

### How watchdog works in current implementation
1. If execution exists in BatchJobs DB, response uses `sourceOfTruth = "BatchJobs"` and `startupWatchdog = null`.
2. If execution does not exist and a trigger attempt is found, API computes a startup projection:
   - `startupSlaSeconds = 600` in production
   - `startupSlaSeconds = 30` in non-production (current code)
3. Projected states are:
   - `TriggerAccepted`
   - `WorkerProcessStarted`
   - `StartFailedTimeout`
   - `WorkerProcessExited` (or mapped terminal outcomes via `workerExitCode` such as `Completed`, `Cancelled`, `Skipped`)

### Trigger attempt store behavior
Current behavior:
- Trigger attempts are stored via `ITriggerAttemptStore`.
- This repo currently wires `MemoryTriggerAttemptStore` with `IMemoryCache`.
- TTL is controlled by `TriggerStore:EntryTtlMinutes` (default `60`).
- Status response exposes `startupWatchdog.triggerStore = "PactInMemoryCache"`.

Operational implication:
- In-memory cache is process-local and volatile.
- Restarting PACT API clears trigger-attempt context for in-flight watchdog projections.
- Multi-instance deployments can produce inconsistent watchdog visibility without a shared store.

Production recommendation:
- Use a distributed implementation of `ITriggerAttemptStore` backed by Redis.
- Keep the same contract fields and TTL policy.
- Route all instances to the same Redis cache so watchdog behavior is consistent across replicas.

Pseudo-code for production store selection:
```csharp
if (environment.IsProduction())
{
  services.AddSingleton<ITriggerAttemptStore, RedisTriggerAttemptStore>();
}
else
{
  services.AddSingleton<ITriggerAttemptStore, MemoryTriggerAttemptStore>();
}
```

---

## Database Schema (Read-Only from PACT)

### Table: fps.job_status (Reference)
```sql
CREATE TABLE IF NOT EXISTS fps.job_status (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);

-- Seeded values per job:
-- INSERT INTO fps.job_status (jobid, status) VALUES 
--   (..., 'Pending'),
--   (..., 'Running'),
--   (..., 'Completed'),
--   (..., 'Failed'),
--   (..., 'Cancelled'),
--   (..., 'Retry'),
--   (..., 'Skipped');
```

### Table: fps.job_queue (Read-Only Query Endpoint)
```sql
CREATE TABLE IF NOT EXISTS fps.job_queue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobexecutionid UUID NOT NULL,  ← Correlation ID from trigger
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,     ← Foreign key to fps.job_status.statusid
    requestedby VARCHAR(100) NOT NULL,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- PACT Query Example:
-- SELECT js.status, jq.*
-- FROM fps.job_queue jq
-- JOIN fps.job_status js ON jq.statusid = js.statusid
-- WHERE jq.jobexecutionid = @executionId
-- ORDER BY jq.updated_at DESC
-- LIMIT 1;
```

---

## API Response Contract

### Trigger Success Response (HTTP 202)
```json
{
  "accepted": true,
  "source": "pact.api",
  "jobName": "RecreateSummaries",
  "jobExecutionId": "7fdf872e2b5841fe887b6bdbb8ea596a",
  "eventId": "localproc-11080",
  "workerPid": 11080,
  "workerProcessLaunched": true,
  "status": "WorkerProcessStarted",
  "acceptedAtUtc": "2026-06-04T10:22:11.341Z",
  "message": "Trigger accepted and local worker process launched. Attach debugger to workerPid."
}
```

### Status Response (HTTP 200) - With Execution
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "7fdf872e-2b58-41fe-887b-6bdbb8ea596a",
  "lastExecution": {
    "executionId": 12345,
    "jobName": "RecreateSummaries",
    "jobExecutionId": "7fdf872e-2b58-41fe-887b-6bdbb8ea596a",
    "status": "Running",
    "startedAt": "2026-06-04T10:22:20.000Z",
    "completedAt": null,
    "durationSeconds": null,
    "recordsProcessed": 1250,
    "recordsFailed": 0,
    "errorMessage": null
  },
  "startupWatchdog": null
}
```

### Status Response (HTTP 200) - During Watchdog Phase
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "7fdf872e2b5841fe887b6bdbb8ea596a",
  "lastExecution": null,
  "startupWatchdog": {
    "projectedState": "WorkerProcessStarted",
    "acceptedAtUtc": "2026-06-04T10:22:11.341Z",
    "startupDeadlineUtc": "2026-06-04T10:22:41.341Z",
    "evaluatedAtUtc": "2026-06-04T10:22:16.211Z",
    "startupSlaSeconds": 30,
    "deliveryExhaustionConfirmed": false,
    "deliveryExhaustionOwner": "IntegrationTransportReconciler",
    "eventId": "localproc-11080",
    "triggerStatus": "WorkerProcessStarted",
    "workerExitCode": null,
    "triggerStore": "PactInMemoryCache"
  }
}
```

---

## Implementation Checklist

### Phase 1: API Endpoints (Week 1)
- [ ] POST /api/v1/batch-jobs/trigger
  - [ ] Accept JSON body with jobName, requestedBy
  - [ ] Validate job name in db
  - [ ] Check for concurrent running (HTTP 409)
  - [ ] Return HTTP 202 with eventId + acceptedAtUtc
  
- [ ] GET /api/batch-jobs/{jobName}/status
  - [ ] Query fps.job_queue for last execution
  - [ ] Join fps.job_status to get state name
  - [ ] Compute watchdog projection
  - [ ] Return full contract response

### Phase 2: Watchdog Logic (Week 1-2)
- [ ] Implement watchdog algorithm
- [ ] Test SLA deadline logic (30s non-prod in current code, 600s prod)
- [ ] Test timeout projection after deadline
- [ ] Test watchdog returns null when execution found

### Phase 3: Testing & Validation (Week 2)
- [ ] Run against Sample UI
- [ ] Execute 4 demo scenarios (happy path, timeout, retry, concurrent)
- [ ] Validate all 7 state values returned
- [ ] Load test concurrent polling (2-5s intervals)
- [ ] Contract sign-off

### Phase 4: Documentation & Handoff (Week 2-3)
- [ ] Update API documentation (Swagger/OpenAPI)
- [ ] Team training on state machine
- [ ] Contract review with BatchJobs team
- [ ] Official sign-off by both teams

---

## Testing Scenarios

All scenarios defined in [DEMO-END-TO-END-FLOW.md](DEMO-END-TO-END-FLOW.md). Run against Sample UI at `http://localhost:5003`:

1. **Happy Path** (60 sec) - Trigger → Pending → Running → Completed
2. **Watchdog Timeout** (190 sec) - Trigger → (no DB) → StartFailedTimeout
3. **Retry Flow** (120 sec) - Running → Failed → Retry → Pending → Running → Completed
4. **Concurrent Rejection** (1 sec) - 2nd trigger returns 409 Conflict

**Success Criteria:**
- All state transitions visible in Sample UI timeline
- Polling intervals respected (2-5s startup, 15-30s running)
- Raw PACT responses match contract schema
- Zero missed state transitions

---

## Quick Start for PACT Developers

1. **Read this document** (10 min)
2. **Study BATCHJOBS-STATE-MACHINE-CONTRACT.md** (30 min)
3. **Review PACT-API-OPENAPI.yaml** (15 min)
4. **Implement endpoints** using OpenAPI as spec
5. **Implement watchdog algorithm** (pseudo-code provided)
6. **Run Sample UI** (http://localhost:5003) for validation
7. **Sign contract** when complete

**Questions?** Ask PACT API Quick Reference section or consult full contract.

---

## Success Criteria (Must Meet Before Deployment)

- [ ] Endpoint 1 (trigger) returns HTTP 202 with eventId + acceptedAtUtc
- [ ] Endpoint 2 (status) returns full contract response (lastExecution + watchdog)
- [ ] All 7 states correctly queried from fps.job_queue/fps.job_status
- [ ] Watchdog logic correctly computes SLA deadline
- [ ] HTTP 409 returned when concurrent trigger detected
- [ ] Sample UI polling works end-to-end
- [ ] 4 demo scenarios pass without modification
- [ ] Load test: 10 concurrent clients polling every 2s for 5 minutes
- [ ] Contract signed by PACT API Lead + BatchJobs Lead
- [ ] Production SLA set to 600 seconds (default or configurable)

---

## Support & Escalation

| Issue | Contact | Response Time |
|-------|---------|----------------|
| Contract clarification | BatchJobs Architect | 24 hours |
| Database schema questions | BatchJobs DBA | 12 hours |
| OpenAPI spec issues | PACT Tech Lead | 24 hours |
| Watchdog algorithm questions | BatchJobs Lead | 24 hours |
| Urgent bugs during implementation | Either team lead | ASAP |

---

## Sign-Off

By signing below, both teams commit to implementing this contract exactly as specified. Deviations require written approval from both leads.

**BatchJobs Team**
- Name: _________________________ 
- Date: _________________________
- Signature: _____________________

**PACT API Team**
- Name: _________________________
- Date: _________________________
- Signature: _____________________

**Architecture Review**
- Name: _________________________
- Date: _________________________
- Signature: _____________________

---

**Contract Version**: 1.0  
**Date Issued**: 2026-06-03  
**Effective Date**: Upon signature  
**Next Review**: After first production deployment
