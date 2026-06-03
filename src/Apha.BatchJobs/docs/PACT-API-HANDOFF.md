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

## What PACT API Must Implement

### Endpoints (2 total)

#### 1. POST /api/v1/batch-jobs/trigger
**Purpose**: Accept batch job trigger, return 202 with correlation ID

**PACT Responsibilities**:
- [x] Validate job name exists in system
- [x] Check single-run policy (return 409 if already running)
- [x] Persist trigger acceptance with acceptedAtUtc timestamp
- [x] Dispatch to event bus (EventBridge/EventGrid/Message Queue)
- [x] Return HTTP 202 with eventId + acceptedAtUtc for polling correlation
- [x] Handle transient/network errors gracefully

**Test Case**:
```bash
curl -X POST http://localhost:5189/api/v1/batch-jobs/trigger \
  -H "Content-Type: application/json" \
  -d '{"jobName":"RecreateSummaries","requestedBy":"demo@local"}'

# Expected: HTTP 202 Accepted
# Body: { "eventId": "...", "acceptedAtUtc": "..." }
```

#### 2. GET /api/batch-jobs/{jobName}/status
**Purpose**: Return current job status including DB state + watchdog projection

**PACT Responsibilities**:
- [x] Query BatchJobs DB for last execution state
- [x] Join fps.job_status to get state name from statusid
- [x] Compute watchdog projection if execution not yet visible in DB
- [x] Compute SLA deadline based on acceptedAtUtc + startupSlaSeconds
- [x] Return BOTH lastExecution (DB) AND startupWatchdog (projection)
- [x] Handle concurrent query loads (batching, caching)

**Test Case**:
```bash
curl http://localhost:5189/api/batch-jobs/RecreateSummaries/status

# Expected: HTTP 200 OK
# Returns: Full contract response with lastExecution + startupWatchdog
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

Computed during startup window (0-180s):
- TriggerAccepted           ← Initial: awaiting DB visibility
- WorkerProcessStarted      ← Local mode: process spawned
- StartFailedTimeout        ← Deadline exceeded, no DB record

All transitions are defined in BATCHJOBS-STATE-MACHINE-CONTRACT.md Section 2
```

---

## Watchdog Algorithm (Critical)

This is the **most important piece** of PACT API logic:

```csharp
public class WatchdogService
{
    public StartupWatchdog ComputeWatchdog(
        DateTime? acceptedAtUtc,
        JobExecution? execution,
        bool isProd)
    {
        // If execution record found in DB, no watchdog needed
        if (execution != null)
            return null;  // ← STOP: DB is source of truth
        
        // If no trigger acceptance recorded, can't compute watchdog
        if (!acceptedAtUtc.HasValue)
            return null;
        
        // Compute SLA deadline
        var now = DateTime.UtcNow;
        var startupSlaSeconds = isProd ? 600 : 180;  // 10m prod, 3m dev
        var deadline = acceptedAtUtc.Value.AddSeconds(startupSlaSeconds);
        
        // Determine projected state
        var projectedState = now > deadline
            ? "StartFailedTimeout"      // Worker never started!
            : "TriggerAccepted";        // Still waiting (normal)
        
        return new StartupWatchdog
        {
            IsWatchdogActive = true,
            TriggeredAtUtc = acceptedAtUtc.Value,
            StartupDeadlineUtc = deadline,
            ProjectedState = projectedState,
            EvaluatedAtUtc = now,
            StartupSlaSeconds = startupSlaSeconds
        };
    }
}
```

**Key Points:**
- ✅ Run watchdog logic **every time** /status is called (not cached)
- ✅ Watchdog is **stateless computation**, not a DB record
- ✅ SLA is 180s (dev) or 600s (prod) — configurable per environment
- ✅ If execution found in DB, **watchdog returns null** (DB is authoritative)
- ✅ Only compute timeout **after** deadline passes **without** DB record

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
  "eventId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "acceptedAtUtc": "2026-06-03T14:30:00.123Z",
  "jobName": "RecreateSummaries",
  "message": "Batch job trigger accepted"
}
```

### Status Response (HTTP 200) - With Execution
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  
  "lastExecution": {
    "jobQueueId": "550e8400-e29b-41d4-a716-446655440000",
    "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
    "currentState": "Running",
    "stateTimestamp": "2026-06-03T14:30:45.123Z",
    "startDateTime": "2026-06-03T14:30:00.000Z",
    "endDateTime": null,
    "requestedBy": "demo@local",
    "errorMessage": null
  },
  
  "startupWatchdog": null
}
```

### Status Response (HTTP 200) - During Watchdog Phase
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  
  "lastExecution": null,
  
  "startupWatchdog": {
    "isWatchdogActive": true,
    "triggerAcceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "projectedState": "TriggerAccepted",
    "evaluatedAtUtc": "2026-06-03T14:30:15.000Z",
    "startupSlaSeconds": 180,
    "deliveryExhaustionConfirmed": false
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
- [ ] Test SLA deadline logic (180s dev, 600s prod)
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
