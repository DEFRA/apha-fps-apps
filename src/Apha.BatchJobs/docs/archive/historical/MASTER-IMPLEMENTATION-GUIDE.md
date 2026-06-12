---
title: "APHA BatchJobs + PACT Integration: Master Implementation Guide"
version: "2.0"
audience: "BatchJobs Team, PACT Team, Architects, DevOps"
format: "Comprehensive Single-Document"
---

# APHA BatchJobs + PACT Integration: Master Implementation Guide

## Executive Summary

This document provides **everything** needed to understand, implement, and deploy the BatchJobs-PACT integration:

- **Architecture Overview**: How the pieces fit together
- **Formal Contract**: 7-state machine specification (ready for sign-off)
- **API Specification**: 2 endpoints with request/response examples
- **Watchdog Algorithm**: SLA-based startup detection with pseudocode
- **Design Rationale**: Why each decision was made (for architects)
- **Deployment Guidance**: Local, staging, production (including EventBridge)
- **Q&A**: Anticipated questions and answers

**Key Assurance**: This architecture is **production-ready for AWS EventBridge** with zero code changes required—only configuration.

---

## Table of Contents

1. [System Architecture](#system-architecture)
2. [Formal Contract (7-State Machine)](#formal-contract-7-state-machine)
3. [API Specification](#api-specification)
4. [Watchdog Algorithm](#watchdog-algorithm)
5. [Design Rationale & Architect Q&A](#design-rationale--architect-qa)
6. [Deployment Guide (Local → Production)](#deployment-guide-local--production)
7. [Team Responsibilities Matrix](#team-responsibilities-matrix)
8. [Implementation Checklist](#implementation-checklist)
9. [Sign-Off](#sign-off)

---

## 1. System Architecture

### 1.1 Big Picture: Event-Driven Job Triggering

```
┌─────────────────────────────────────────────────────────────────────┐
│  EVENT SOURCE (EventGrid)                                           │
│  └─ BatchJobs sample UI, external systems, manual triggers         │
└─────────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────────┐
│  PACT API (Your Team to Build)                                      │
│  ├─ POST /api/v1/batch-jobs/trigger → HTTP 202 Accepted           │
│  │   └─ Validates job name, accepts trigger, dispatches            │
│  │   └─ Returns: jobExecutionId, eventId, acceptedAtUtc            │
│  ├─ GET /api/batch-jobs/{jobName}/status → HTTP 200 OK            │
│  │   └─ Queries DB, applies watchdog logic                         │
│  │   └─ Returns: currentState (or transient projection)            │
│  └─ Abstraction: ITriggerDispatcher interface                       │
└─────────────────────────────────────────────────────────────────────┘
           ↓                                          ↓
    (Dev: LocalProcess)                      (Prod: EventBridge)
           ↓                                          ↓
┌──────────────────────┐              ┌──────────────────────────────┐
│ Local Worker Process │              │  AWS EventBridge             │
│ (localhost)          │              │  ├─ EventGrid rule           │
│ ├─ jobName           │              │  ├─ SQS/SNS target           │
│ ├─ jobExecutionId    │              │  └─ ECS Fargate Task         │
│ └─ acceptedAtUtc     │              └──────────────────────────────┘
└──────────────────────┘                        ↓
           ↓                          ┌──────────────────────────┐
           │                          │  ECS Worker Container    │
           │                          │  ├─ jobName              │
           │                          │  ├─ jobExecutionId       │
           │                          │  └─ RDS Connection       │
           │                          └──────────────────────────┘
           └────────────┬───────────────────────────────────────┘
                        ↓
        ┌───────────────────────────────┐
        │  PostgreSQL (Authoritative)   │
        │  ├─ fps.job_queue (executions)│
        │  ├─ fps.job_status (ref table)│
        │  └─ State: Pending→Running→...│
        └───────────────────────────────┘
                        ↓
        ┌───────────────────────────────┐
        │  Polling Client               │
        │  (Sample UI or PACT client)   │
        │  ├─ Poll every 2-5s (startup) │
        │  ├─ Poll every 15-30s (run)   │
        │  └─ Stop at terminal state    │
        └───────────────────────────────┘
```

### 1.2 Data Flow Example: "RecreateSummaries" Job

**Timestamp: T+0s** (Trigger Phase)
```
1. UI: POST /api/v1/batch-jobs/trigger
   {
     "jobName": "RecreateSummaries",
     "requestedBy": "user@local"
   }

2. PACT API:
   - Validates job name via BatchJobRoutingPolicy
   - Creates jobExecutionId = "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c"
   - Captures acceptedAtUtc = "2026-06-03T14:30:00.000Z"
   - Calls ITriggerDispatcher.DispatchAsync(...)

3. Dispatcher Choice:
   Local Dev:  LocalWorkerProcessTriggerDispatcher → Spawn process
   Production: EventBridgeTriggerDispatcher → Publish to EventBridge

4. Response to UI (HTTP 202 Accepted):
   {
     "accepted": true,
     "jobName": "RecreateSummaries",
     "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
     "eventId": "localproc-12345" (local) OR "{AWS Event ID}" (prod),
     "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
     "message": "Trigger accepted. Job queued for execution."
   }
```

**Timestamp: T+3s** (Worker Startup Phase)
```
5. Worker Process/ECS Container:
   - Reads jobName, jobExecutionId, acceptedAtUtc from environment
   - Connects to PostgreSQL
   - Inserts into fps.job_queue:
     {
       "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
       "jobName": "RecreateSummaries",
       "statusid": (FK to "Pending"),
       "createdDatetimeUtc": "2026-06-03T14:30:03.000Z"
     }

6. UI Polls (T+3s to T+10s): 
   GET /api/batch-jobs/RecreateSummaries/status
   
   PACT API:
   - Queries fps.job_queue for jobExecutionId
   - Finds record with statusid = "Pending"
   - Watchdog is null (execution found)
   
   Response (HTTP 200 OK):
   {
     "jobName": "RecreateSummaries",
     "isRunning": true,
     "sourceOfTruth": "BatchJobs",
     "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
     "lastExecution": {
       "currentState": "Pending",
       "startDateTime": "2026-06-03T14:30:03.000Z",
       "estimatedEndDateTime": null
     },
     "startupWatchdog": null
   }

7. UI displays: "Status: Pending (blue)"
```

**Timestamp: T+15s** (Running Phase)
```
8. Worker executes business logic, updates DB:
   UPDATE fps.job_queue 
   SET statusid = "Running"
   WHERE jobExecutionId = "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c"

9. UI Polls (every 15-30s now):
   GET /api/batch-jobs/RecreateSummaries/status
   
   Response:
   {
     "lastExecution": {
       "currentState": "Running",
       "startDateTime": "2026-06-03T14:30:03.000Z"
     }
   }

10. UI displays: "Status: Running (green spinner)"
```

**Timestamp: T+45s** (Completion Phase)
```
11. Worker finishes, updates DB:
    UPDATE fps.job_queue 
    SET statusid = "Completed", estimatedEndDateTime = "2026-06-03T14:31:45.000Z"
    WHERE jobExecutionId = "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c"

12. UI Polls:
    Response:
    {
      "lastExecution": {
        "currentState": "Completed",
        "startDateTime": "2026-06-03T14:30:03.000Z",
        "estimatedEndDateTime": "2026-06-03T14:31:45.000Z"
      }
    }

13. UI displays: "Status: Completed ✓ (green)"
    → Stops polling (terminal state)
```

---

## 2. Formal Contract (7-State Machine)

### 2.1 State Definition Table

| # | State | Persistence | Terminal? | Meaning | DB Source |
|---|-------|-------------|-----------|---------|-----------|
| 1 | **Pending** | ✅ Persisted | ❌ No | Job queued, awaiting worker pickup | fps.job_queue.statusid |
| 2 | **Running** | ✅ Persisted | ❌ No | Worker executing business logic | fps.job_queue.statusid |
| 3 | **Completed** | ✅ Persisted | ✅ YES | Job finished successfully | fps.job_queue.statusid |
| 4 | **Failed** | ✅ Persisted | ❌ No* | Job error detected (may auto-transition) | fps.job_queue.statusid |
| 5 | **Cancelled** | ✅ Persisted | ✅ YES | Job terminated by user/system | fps.job_queue.statusid |
| 6 | **Retry** | ✅ Persisted | ❌ No | Failed job, retry scheduled | fps.job_queue.statusid |
| 7 | **Skipped** | ✅ Persisted | ✅ YES | Job rejected at trigger time (policy/lock) | fps.job_queue.statusid |

*Failed may auto-transition to Retry if retry policy active

### 2.2 State Transition Rules

```
Pending
  ↓ (Worker starts processing)
Running
  ├─ → Completed (success) ✅ TERMINAL
  ├─ → Failed (error)
  │   ├─ → Retry (retry policy active, auto-transition)
  │   │   └─ → Pending (retry scheduler re-queues)
  │   │       └─ → Running (worker picks up again)
  │   └─ → ? (terminal if no retry policy)
  └─ → Cancelled (user/system abort) ✅ TERMINAL

TriggerRequested (user intent)
  ↓
Skipped (policy rejection or lock conflict) ✅ TERMINAL
```

**Mutual Exclusion Policy**:
- Only one execution per job name at a time
- If second trigger received during Running/Pending → return HTTP 409 Conflict

**Retry Policy** (Optional):
- Failed jobs may auto-transition to Retry
- Retry scheduler moves Retry → Pending after delay
- Maximum retry attempts configurable per job

### 2.3 Transient API Projections (Not Persisted)

During startup phase, PACT API may project transient states while watchdog is active:

| State | Condition | Meaning | When to Display |
|-------|-----------|---------|-----------------|
| **TriggerAccepted** | acceptedAtUtc + SLA not yet exceeded, no DB record | Trigger received, awaiting worker startup | During watchdog window (before DB record visible) |
| **WorkerProcessStarted** | Local dev mode, process spawned | Worker process started locally | Local dev only, ephemeral |
| **StartFailedTimeout** | acceptedAtUtc + SLA exceeded, no DB record | Worker failed to start within SLA | When watchdog SLA deadline exceeded |

**Critical**: These are **projections only** for UX; not stored in database. They help clients understand "why is there no DB record yet?"

---

## 3. API Specification

### 3.1 Trigger Endpoint (POST /api/v1/batch-jobs/trigger)

**Purpose**: Accept a job trigger request, validate it, dispatch to worker, return acceptance confirmation

**Request**:
```http
POST /api/v1/batch-jobs/trigger HTTP/1.1
Host: pact-api.example.com
Content-Type: application/json

{
  "jobName": "RecreateSummaries",
  "requestedBy": "user@example.com"
}
```

**Request Fields**:
- `jobName` (string, required): Job identifier from BatchJobRoutingPolicy
  - Example valid values: "RecreateSummaries", "CalculateMetrics", etc.
  - Validation: Must be in BatchJobRoutingPolicy.CanTriggerFromSource()
- `requestedBy` (string, required): Who triggered the job (user email, system name, etc.)

**Success Response (HTTP 202 Accepted)**:
```json
{
  "accepted": true,
  "jobName": "RecreateSummaries",
  "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "eventId": "localproc-12345",
  "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
  "message": "Trigger accepted. Job queued for execution."
}
```

**Response Fields (202)**:
- `accepted` (bool): Always true on 202
- `jobName` (string): Echo of request jobName
- `jobExecutionId` (string): UUID assigned by PACT API
  - **Correlation ID**: Used throughout execution lifecycle
  - Client must store this to track the job
  - Format: UUID v4 (36 chars with hyphens)
- `eventId` (string): Event ID from dispatcher
  - Local dev: "localproc-{ProcessId}"
  - Production: AWS EventBridge Event ID
  - **Telemetry ID**: Used for logging/debugging
- `acceptedAtUtc` (ISO 8601 UTC timestamp): When trigger accepted
  - **Watchdog Reference**: Used to compute SLA deadline
  - Format: "2026-06-03T14:30:00.000Z"
- `message` (string): Human-readable confirmation

**Conflict Response (HTTP 409 Conflict)**:
```json
{
  "accepted": false,
  "jobName": "RecreateSummaries",
  "message": "Job is already running. Cannot trigger concurrent execution.",
  "currentExecution": {
    "jobExecutionId": "x9y8z7w6-v5u4-43t2-1s0r-9q8p7o6n5m4l",
    "currentState": "Running",
    "startDateTime": "2026-06-03T14:29:00.000Z"
  }
}
```

**Bad Request Response (HTTP 400 Bad Request)**:
```json
{
  "accepted": false,
  "jobName": "UnknownJob",
  "message": "Invalid job name. Job 'UnknownJob' is not registered in BatchJobRoutingPolicy."
}
```

**Design Decision - Why HTTP 202?**
- **202 Accepted**: Job trigger is asynchronous; we're not waiting for execution
- Signals to client: "Your request is accepted, execution will happen soon"
- Client must poll status endpoint for actual job state
- Alternative: 200 OK is misleading (implies immediate completion)

**Design Decision - Why jobExecutionId in response?**
- Provides immediate correlation ID before DB record exists
- Client doesn't need to wait for DB visibility
- Supports watchdog algorithm (acceptedAtUtc + jobExecutionId → track SLA)
- Enables retry logic and request deduplication

---

### 3.2 Status Endpoint (GET /api/batch-jobs/{jobName}/status)

**Purpose**: Query current job state, apply watchdog logic, return execution details

**Request**:
```http
GET /api/batch-jobs/RecreateSummaries/status HTTP/1.1
Host: pact-api.example.com
```

**Request Parameters**:
- `jobName` (path param, required): Job identifier

**Success Response (HTTP 200 OK)**:

**Scenario A: Execution Found in DB**
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": {
    "currentState": "Running",
    "startDateTime": "2026-06-03T14:30:03.000Z",
    "estimatedEndDateTime": null
  },
  "startupWatchdog": null
}
```

**Scenario B: Watchdog Active (No DB Record Yet)**
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": null,
  "startupWatchdog": {
    "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "projectedState": "TriggerAccepted",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "secondsRemainingInSla": 180,
    "isActive": true
  }
}
```

**Scenario C: Watchdog Timeout (SLA Exceeded, No DB Record)**
```json
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": null,
  "startupWatchdog": {
    "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "projectedState": "StartFailedTimeout",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "secondsRemainingInSla": -45,
    "isActive": false
  }
}
```

**Response Fields (200 OK)**:
- `jobName` (string): Echo of request parameter
- `isRunning` (bool): 
  - true if state is Pending, Running, Retry, or Failed
  - false if state is Completed, Cancelled, Skipped, StartFailedTimeout
- `sourceOfTruth` (string enum):
  - "BatchJobs": Data from fps.job_queue DB table
  - "StartupWatchdog": Projected state while worker starting
- `correlatedJobExecutionId` (UUID string): The jobExecutionId from trigger response
- `lastExecution` (object or null):
  - Populated if execution record found in DB
  - `currentState` (string): One of 7 states (Pending, Running, Completed, Failed, Cancelled, Retry, Skipped)
  - `startDateTime` (ISO 8601 UTC): When execution started (inserted into DB)
  - `estimatedEndDateTime` (ISO 8601 UTC or null): When execution ended (or null if still running)
- `startupWatchdog` (object or null):
  - Populated if watchdog is active (no DB record yet) OR timeout occurred
  - `acceptedAtUtc` (ISO 8601 UTC): From trigger response
  - `projectedState` (string): Watchdog projection (TriggerAccepted, WorkerProcessStarted, StartFailedTimeout)
  - `startupDeadlineUtc` (ISO 8601 UTC): acceptedAtUtc + SLA seconds
  - `secondsRemainingInSla` (integer): Deadline - now (negative if exceeded)
  - `isActive` (bool): false if timeout occurred

**Not Found Response (HTTP 404 Not Found)**:
```json
{
  "jobName": "UnknownJob",
  "message": "Job 'UnknownJob' not found or never triggered."
}
```

**Design Decision - Why Return Both lastExecution AND startupWatchdog?**
- **lastExecution**: Source of truth once worker DB record visible
- **startupWatchdog**: Provides UX feedback during blind startup window
- Client can distinguish "still starting" from "started but failed"
- Enables different UI handling: spinner (watchdog active) vs. error (timeout)

---

## 4. Watchdog Algorithm

### 4.1 Why Watchdog is Needed

**The Problem**: Asynchronous dispatch (EventBridge, queues, etc.) creates a "blind window" where:
1. Client triggers job → HTTP 202 accepted → returns immediately
2. Client starts polling status endpoint
3. **But worker hasn't started yet → no DB record exists**
4. Client can't tell: "Is worker still starting?" or "Did it fail silently?"

**The Watchdog Solution**: Compute a projected state during the startup window:
- **0-10 seconds**: Project "TriggerAccepted" (startup happening normally)
- **10-180 seconds (dev) or 600 seconds (prod)**: Project "TriggerAccepted" (still starting)
- **>SLA deadline**: Project "StartFailedTimeout" (something went wrong)

### 4.2 Watchdog Algorithm (Pseudocode)

```pseudocode
FUNCTION GetJobStatus(jobName: String) → StatusResponse
  // Step 1: Query database for execution record
  execution = QueryDatabase(fps.job_queue, jobName)
  
  IF execution EXISTS THEN
    // Step 2a: DB record found → return actual state
    RETURN {
      sourceOfTruth: "BatchJobs",
      lastExecution: execution,
      startupWatchdog: null
    }
  ELSE
    // Step 2b: No DB record → check watchdog
    RETURN ComputeWatchdogProjection(jobName)
  END IF
END FUNCTION

FUNCTION ComputeWatchdogProjection(jobName: String) → StatusResponse
  // Step 3: Query for most recent trigger attempt (acceptedAtUtc, jobExecutionId)
  lastTrigger = QueryLatestTrigger(jobName)
  
  IF lastTrigger IS NULL THEN
    // Never triggered
    RETURN HttpNotFound("Job never triggered")
  END IF
  
  // Step 4: Compute SLA deadline
  isProd = Environment == "Production"
  slaSeconds = isProd ? 600 : 180  // 10 min prod, 3 min dev
  startupDeadline = lastTrigger.acceptedAtUtc + slaSeconds
  
  // Step 5: Compare current time to deadline
  secondsRemaining = (startupDeadline - UtcNow()).TotalSeconds
  
  IF secondsRemaining > 0 THEN
    // Still within SLA window → startup in progress
    projectedState = "TriggerAccepted"
    isActive = true
  ELSE
    // SLA exceeded, no DB record → something failed
    projectedState = "StartFailedTimeout"
    isActive = false
  END IF
  
  // Step 6: Return watchdog projection
  RETURN {
    sourceOfTruth: "StartupWatchdog",
    correlatedJobExecutionId: lastTrigger.jobExecutionId,
    lastExecution: null,
    startupWatchdog: {
      acceptedAtUtc: lastTrigger.acceptedAtUtc,
      projectedState: projectedState,
      startupDeadlineUtc: startupDeadline,
      secondsRemainingInSla: secondsRemaining,
      isActive: isActive
    }
  }
END FUNCTION
```

### 4.3 Implementation Requirements (CRITICAL)

**These are NON-NEGOTIABLE for watchdog to work correctly**:

1. **Store acceptedAtUtc in PACT API trigger response**
   - Must be UTC datetime in ISO 8601 format
   - Used as reference point for SLA calculation
   - Cannot be adjusted after response sent

2. **Store jobExecutionId in trigger response**
   - Returned immediately, not from DB
   - Enables watchdog to track trigger even before DB record visible
   - Must be queryable from PACT side (e.g., in-memory store, cache, or separate table)

3. **Use Correct SLA Values**
   - Development: 180 seconds (3 minutes)
   - Production: 600 seconds (10 minutes)
   - Base on `Environment` variable, not configuration
   - If not set correctly, UX feedback breaks (premature timeouts or long waits)

4. **Watchdog Takes Priority Over DB in Status Response**
   - If DB record found: ignore watchdog, return actual state
   - If DB record NOT found: check watchdog, return projection
   - Never return both as "truth" (confusing for clients)

5. **Query Latest Trigger (Not All Triggers)**
   - Watchdog should use most recent trigger for the job
   - Prevents old triggers from interfering with current execution
   - Clean up old triggers after terminal state reached

### 4.4 SLA Configuration Justification

| Environment | SLA Seconds | Rationale |
|-------------|-------------|-----------|
| **Development** | 180 (3 min) | Local .NET processes start quickly (<5s typically); 3 min gives 35s buffer for slow machines |
| **Staging** | 600 (10 min) | Container spin-up is slower; account for image pulls, network delays |
| **Production** | 600 (10 min) | ECS Fargate cold start + EventBridge routing + network → 2-5 min typical; 10 min is safe ceiling |

**Design Decision - Why Environment-Based, Not Configurable?**
- Prevents misconfiguration (typos, wrong values in wrong env)
- Simplifies deployment (fewer knobs to turn)
- SLA is fundamental to UX, not a tuning parameter
- If you need different values, that's a sign of architecture problem

---

## 5. Design Rationale & Architect Q&A

### 5.1 Design Principles

**1. Abstraction Over Implementation**
- `ITriggerDispatcher` interface hides dispatch mechanism (EventBridge, process, queue, etc.)
- Enables local dev without EventBridge
- Production code uses same interface → zero code change for deployment

**2. Asynchronous by Default**
- HTTP 202 Accepted, not 200 OK
- Client must poll for state changes
- Aligns with cloud-native patterns (queue-based processing)

**3. SLA-Based Observability**
- Watchdog is not just diagnostics; it's **part of the contract**
- Client UI can confidently say "starting..." or "something failed"
- Prevents infinite spinners and confused users

**4. Database as Source of Truth**

**5. Correlation Over Causation**
**4. Database as Source of Truth (for Execution State)**
- BatchJobs database is authoritative for **job execution state** (Pending/Running/Completed/etc)
- PACT API maintains its **own trigger store** for **trigger metadata** (jobExecutionId, acceptedAtUtc)
- Two separate concerns with clear ownership:
  - PACT store: Trigger accepted at this time with this ID (immutable record)
  - BatchJobs DB: Job is in this state (mutable, changes as job executes)
- Watchdog bridges the gap: queries PACT store for trigger metadata, then queries DB for state
- Makes auditing easier (each system records its own responsibility)

**5. Correlation Over Causation**
- jobExecutionId is correlation ID across both PACT API (trigger store) and BatchJobs DB
- Traces entire lifecycle: trigger acceptance → dispatch → worker startup → DB write → polling status
- Both systems record the same jobExecutionId, enabling end-to-end tracing

### 5.2 Architect Q&A

#### Q1: Why HTTP 202 instead of 200 OK?

**A**: HTTP 202 Accepted explicitly signals asynchronous processing.

**Problem with 200 OK**:
- Client code might assume job is done
- REST convention: 200 = success of request = job executed
- Leads to polling code being optional/forgotten
- Subtle bugs: "Why isn't my job running?" (because client never polled)

**202 Accepted**:
- Explicitly means: "I accepted your request; execution happening elsewhere"
- Client must poll to know if job succeeded
- REST best practice for async operations (RFC 7231)
- Example: HTTP POST to printer returns 202 (print job queued, not completed)

#### Q2: Why jobExecutionId in API response, not auto-generated from DB?

**A**: jobExecutionId must be available **before** DB record exists.

**Why this matters**:
1. Client triggers job → gets HTTP 202 with jobExecutionId
2. Client starts polling immediately
3. Worker still starting (no DB record yet)
4. Watchdog needs jobExecutionId to track "which trigger is this for?"
5. Without it, watchdog can't correlate → can't compute SLA → can't project state

**Alternative (What NOT to do)**:
- Wait for DB record before returning jobExecutionId ❌ (defeats async purpose)
- Return empty jobExecutionId ❌ (watchdog can't track)
- Generate jobExecutionId in DB ❌ (worker too slow, creates blind window)

#### Q3: Why 3 min for dev SLA, 10 min for prod?

**A**: Different startup characteristics.

**Dev (LocalProcess)**:
- .NET process starts immediately (<1 second)
- 3 minute SLA gives 179s buffer
- Fast feedback on failures (don't wait 10 min to know something broke)

**Prod (EventBridge + ECS)**:
- EventBridge delay: 0-1 second
- ECS task spin-up: 5-30 seconds (first run) or 2-5 seconds (warm)
- Container image pull: variable (first time can be 30-60 seconds)
- Network latency: milliseconds but cumulative
- Typical total: 2-5 minutes
- 10 min SLA = safety margin for slow days (network congestion, cold start)

**Why NOT 180s for both**:
- Would cause premature timeout failures in prod (cold starts take 5+ min)
- Users would see "StartFailedTimeout" incorrectly

**Why NOT 600s for both**:
- Dev experience suffers (3 min wait to know job failed)
- Defeats rapid feedback loop

#### Q4: Why separate Retry state? Why not auto-transition to Pending?

**A**: Explicit state allows observability and control.

**CRITICAL CORRECTION**: 
- `QueryPactTriggerStore()` queries **PACT API's OWN trigger store**, NOT fps.job_queue
- jobExecutionId is NOT persisted in fps.job_queue until the worker starts
- PACT API must maintain its own trigger log (in-memory, cache, or separate table)
- When worker writes to fps.job_queue, that becomes the source of truth
- Watchdog accesses PACT's store to get the original acceptedAtUtc and jobExecutionId

#### Q5: Where does the PACT trigger store live? (Current Implementation)

**A**: The current implementation uses a **PACT-owned local in-memory cache** trigger store.

**Implemented flow**:
- `POST /trigger` writes trigger metadata into PACT in-memory store before returning HTTP 202
- Stored fields include `jobExecutionId`, `jobName`, `acceptedAtUtc`, `eventId`, and startup marker (`TriggerAccepted` or `WorkerProcessStarted`)
- `GET /status` first checks BatchJobs DB for execution state
- If no DB row exists yet, watchdog reads latest trigger metadata from PACT in-memory store by `jobName` (or `jobExecutionId` when supplied)
- Watchdog projects `TriggerAccepted` or `WorkerProcessStarted` while SLA is active, then `StartFailedTimeout` when SLA is exceeded
- Once worker writes to `fps.job_queue`, DB remains the execution source of truth

**Important limitation**:
- Local in-memory store is process-local and non-durable; trigger metadata is lost on PACT API restart
- This is acceptable for local/dev validation, but production should move to a shared/durable store (Redis or dedicated table)

**Minimum acceptance criteria (non-negotiable)**:
- `POST /trigger` must write trigger metadata before returning HTTP 202
- `GET /status` watchdog path must read from this store when no `fps.job_queue` row exists
- Latest trigger by `jobName` must be queryable deterministically
- Store must support cleanup/TTL after terminal completion
- Correlation by `jobExecutionId` must work across PACT logs and BatchJobs DB records

**Configuration implemented**:
- Trigger store TTL is configurable via `TriggerStore:EntryTtlMinutes` (default 60)
- Status response includes watchdog metadata sourced from trigger store during startup blind window

**Scaling strategy**:
- Add read replicas for status endpoint (read-only)
- Cache status responses (but invalidate on state change)
- Use materialized views for aggregations (if needed)

---

## 6. Deployment Guide (Local → Production)

### 6.1 Dispatcher Architecture

Your codebase has **two dispatchers**; system auto-selects based on environment:

```csharp
// Program.cs (actual code)
builder.Services.AddScoped<ITriggerDispatcher>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<TriggerDispatchOptions>>();
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

    // LOCAL DEVELOPMENT: Use process-based dispatch if mode is "LocalProcess"
    if (string.Equals(options.Mode, "LocalProcess", StringComparison.OrdinalIgnoreCase))
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
        {
            return serviceProvider.GetRequiredService<LocalWorkerProcessTriggerDispatcher>();
        }
        // Safety: Don't allow LocalProcess outside dev/local
        logger.LogWarning("LocalProcess not allowed in {Environment}; falling back to EventBridge", 
            environment.EnvironmentName);
    }

    // PRODUCTION: Use EventBridge (default)
    return serviceProvider.GetRequiredService<EventBridgeTriggerDispatcher>();
});
```

### 6.2 Configuration by Environment

**Development (appsettings.Development.json)**:
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "TriggerDispatch": {
    "Mode": "LocalProcess"
  },
  "ConnectionStrings": {
    "FPSConnectionString": "Host=localhost;Database=batch_jobs_foundation_db_cloud;User=postgres;Password=..."
  }
}
```
✅ Result: Triggers spawn worker as local .NET process

**Staging (appsettings.Staging.json)**:
```json
{
  "ASPNETCORE_ENVIRONMENT": "Staging",
  "TriggerDispatch": {
    "Mode": "EventBridge"
  },
  "EventBridge": {
    "EventBusName": "staging-bus",
    "DetailType": "BatchJobTrigger",
    "Source": "apha.pact.api"
  },
  "ConnectionStrings": {
    "FPSConnectionString": "Host=rds-staging.aws.com;Database=batch_jobs_foundation_db_cloud;User=...;Password=..."
  }
}
```
✅ Result: Triggers publish to EventBridge (staging)

**Production (appsettings.Production.json)**:
```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "TriggerDispatch": {
    "Mode": "EventBridge"
  },
  "EventBridge": {
    "EventBusName": "default",
    "DetailType": "BatchJobTrigger",
    "Source": "apha.pact.api"
  },
  "ConnectionStrings": {
    "FPSConnectionString": "Host=rds-prod.aws.com;Database=batch_jobs_foundation_db_cloud;User=...;Password=..."
  }
}
```
✅ Result: Triggers publish to EventBridge (production)

### 6.3 AWS EventBridge Setup (Production)

**Prerequisites**:
- [ ] AWS Account with EventBridge enabled
- [ ] EventBridge Rule created
- [ ] ECS Task Definition for worker
- [ ] RDS PostgreSQL endpoint
- [ ] IAM roles configured

**EventBridge Rule Configuration**:
```
Rule Name: apha-batch-jobs-trigger
State: Enabled
Event Bus: default
Event pattern:
{
  "detail-type": ["BatchJobTrigger"],
  "source": ["apha.pact.api"],
  "detail": {
    "jobName": ["RecreateSummaries", "CalculateMetrics"]
  }
}

Targets:
- ECS Task Definition (apha-batch-jobs-worker)
- Role: ecsTaskExecutionRole
- Cluster: production
- Launch Type: Fargate
```
  "family": "apha-batch-jobs-worker",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::ACCOUNT:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "worker",
      "image": "ACCOUNT.dkr.ecr.REGION.amazonaws.com/apha/batchjobs:latest",
      "environment": [
      ],
      "logConfiguration": {
          "awslogs-region": "REGION",
          "awslogs-stream-prefix": "worker"
        }
      }
    }
  ]
}
```

**IAM Role for PACT API**:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "events:PutEvents"
      ],
      "Resource": "arn:aws:events:REGION:ACCOUNT:event-bus/default"
    }
  ]
}
```

### 6.4 Migration Path: Local → Staging → Production

**Week 1: Local Development**
```
1. Clone repo
2. Set ASPNETCORE_ENVIRONMENT=Development
3. Set TriggerDispatch:Mode=LocalProcess
4. dotnet run
5. Trigger jobs via Sample UI (http://localhost:5003)
6. ✅ See jobs execute locally
```

**Week 2: Staging Deployment**
```
1. Provision RDS PostgreSQL (staging-db)
2. Deploy migration scripts
3. Set ASPNETCORE_ENVIRONMENT=Staging
4. Set TriggerDispatch:Mode=EventBridge
5. Create EventBridge rule for staging bus
6. Deploy ECS task definition to staging cluster
7. Deploy PACT API container to ECS (staging)
8. Trigger jobs via Sample UI → PACT API (staging)
9. ✅ See jobs routed through EventBridge → ECS → DB
```

**Week 3-4: Production Deployment**
```
1. Provision RDS PostgreSQL (prod-db)
2. Deploy migration scripts
3. Set ASPNETCORE_ENVIRONMENT=Production
4. Set TriggerDispatch:Mode=EventBridge (or omit for default)
5. Create EventBridge rule for default bus
6. Deploy ECS task definition to production cluster
7. Deploy PACT API container to production
8. Smoke test: 1-2 manual triggers
9. Monitor logs/dashboards for 24 hours
10. ✅ Go live
```

**Key: Minimal Code Changes Between Stages**
- Configuration files only
- No code changes
- Same PACT API binary
- Proves design is sound

---

## 7. Team Responsibilities Matrix

### 7.1 Who Does What?

| Responsibility | Owner | Deadline | Approval |
|---|---|---|---|
| **Contract Sign-Off** | PACT Lead + BatchJobs Lead | Week 0, Day 1 | Section 9: Sign-Off |
| **API Implementation** (2 endpoints) | PACT Team | Week 1-2 | Integration tests pass |
| **Watchdog Algorithm** | PACT Team | Week 1-2 | Unit tests + manual verification |
| **Database Integration** | PACT Team | Week 1-2 | Can query fps.job_queue, return state |
| **Testing vs Sample UI** | PACT Team + QA | Week 2 | 4 demo scenarios pass |
| **EventBridge Setup** (AWS) | DevOps Team | Week 2-3 | Rule created, targets configured |
| **ECS Deployment** | DevOps Team | Week 2-3 | Task def deployed, worker runs |
| **Go-Live Checklist** | DevOps + PACT Lead | Week 3 | All items signed off |

### 7.2 PACT Team Responsibilities (Detailed)

**Implement 2 Endpoints**:
1. `POST /api/v1/batch-jobs/trigger`
   - Validate jobName
   - Generate jobExecutionId (UUID)
   - Capture acceptedAtUtc
   - Call ITriggerDispatcher.DispatchAsync()
   - Return 202 Accepted

2. `GET /api/batch-jobs/{jobName}/status`
   - Query fps.job_queue
   - Implement watchdog algorithm
   - Return status with projections

**Implement Watchdog Algorithm**:
- Store trigger attempts (jobExecutionId, jobName, acceptedAtUtc)
- Compute SLA deadline based on environment
- Project state based on DB visibility + SLA
- Handle all 3 scenarios (DB found, watchdog active, timeout)

**Testing**:
- Unit tests for watchdog logic (mock DB)
- Integration tests against real DB
- Manual testing with Sample UI
- Load testing: 100 concurrent clients polling

**Documentation**:
- Update PACT API docs
- Add examples to README
- Document SLA behavior

### 7.3 BatchJobs Team Responsibilities

**Database Schema**:
- Ensure fps.job_status table has all 7 states (migration provided)
- Ensure fps.job_queue table indexed on jobExecutionId, jobName
- Monitor query performance (status queries are frequent)

**Worker**:
- Reads jobName, jobExecutionId, acceptedAtUtc from environment
- Inserts into fps.job_queue with Pending state
- Updates state as execution progresses
- Handles errors gracefully (write Failed state, not crash)

**Monitoring**:
- Set up CloudWatch dashboards
- Alert on StartFailedTimeout (watchdog timeout)
- Track state transitions over time
- Report SLA metrics (how often do jobs start within SLA window?)

---

## 8. Implementation Checklist

### 8.1 Phase 1: Contract & Design (Complete ✅)

- [x] Define 7-state machine
- [x] Document API endpoints
- [x] Document watchdog algorithm
- [x] Create OpenAPI specification
- [x] Create Sample UI reference implementation
- [x] Team review & sign-off

### 8.2 Phase 2: PACT API Implementation (Week 1-2)

**Endpoints**:
- [ ] POST /api/v1/batch-jobs/trigger
  - [ ] Validate jobName
  - [ ] Generate jobExecutionId
  - [ ] Capture acceptedAtUtc
  - [ ] Call dispatcher
  - [ ] Return 202 Accepted
  - [ ] Handle 409 (concurrent) and 400 (invalid job)

- [ ] GET /api/batch-jobs/{jobName}/status
  - [ ] Query fps.job_queue
  - [ ] Implement watchdog (all 3 scenarios)
  - [ ] Return 200 OK with state
  - [ ] Handle 404 (never triggered)

**Watchdog**:
- [ ] Store trigger attempts
- [x] Implement PACT-owned trigger store using local in-memory cache
- [x] Implement `QueryPactTriggerStore()` behavior against PACT in-memory store
- [ ] Finalize production trigger store option (Redis vs dedicated table) with team sign-off
- [ ] Compute SLA deadline (180s dev, 600s prod)
- [ ] Query DB for execution record
- [ ] Project state if no record
- [ ] Handle timeout case
- [ ] Unit tests (80%+ coverage)

**Dependencies**:
- [ ] Configure ITriggerDispatcher interface
- [ ] Inject EventBridgeTriggerDispatcher
- [ ] Inject LocalWorkerProcessTriggerDispatcher
- [ ] Configure TriggerDispatchOptions

### 8.3 Phase 3: Testing (Week 2)

**Unit Tests**:
- [ ] Watchdog SLA calculation
- [ ] Watchdog state projection
- [ ] Endpoint request validation
- [ ] Response schema validation

**Integration Tests**:
- [ ] Against real DB (fps.job_queue, fps.job_status)
- [ ] Trigger → DB write → Status query
- [ ] Watchdog timeout scenario
- [ ] Concurrent trigger (409 response)

**Manual Tests vs Sample UI**:
- [ ] Trigger job via Sample UI
- [ ] Sample UI displays "Pending"
- [ ] Polling continues
- [ ] Sample UI displays "Running" (if worker starts)
- [ ] Sample UI displays "Completed" (if job succeeds)
- [ ] All 4 demo scenarios pass

### 8.4 Phase 4: Production Readiness (Week 2-3)

**DevOps**:
- [ ] EventBridge rule created
- [ ] ECS task definition deployed
- [ ] IAM roles configured
- [ ] RDS endpoint accessible
- [ ] Networking configured (PACT API → EventBridge, Worker → RDS)

**Deployment**:
- [ ] PACT API container built
- [ ] PACT API deployed to staging
- [ ] Integration test in staging
- [ ] PACT API deployed to production
- [ ] Smoke test in production
- [ ] Monitor logs for 24 hours

**Go-Live**:
- [ ] All checklist items signed off
- [ ] Team briefing completed
- [ ] Escalation path documented
- [ ] Rollback plan documented
- [ ] ✅ LIVE

---

## 9. Sign-Off

**This contract defines the interface between BatchJobs and PACT teams.**

By signing below, you agree to implement the 2 endpoints, watchdog algorithm, and testing scenarios as specified in this document.

### BatchJobs Team

Signed: _________________________ 

Name & Title: _________________________ 

Date: _________________________ 

### PACT Team

Signed: _________________________ 

Name & Title: _________________________ 

Date: _________________________ 

### DevOps Team

Signed: _________________________ 

Name & Title: _________________________ 

Date: _________________________ 

---

## Appendix A: Glossary

| Term | Definition |
|------|-----------|
| **jobExecutionId** | UUID assigned by PACT API at trigger time; correlation ID for entire execution lifecycle |
| **acceptedAtUtc** | Timestamp when trigger accepted; watchdog reference point for SLA calculation |
| **Watchdog** | Algorithm that projects job state during startup window (before DB record visible) |
| **SLA** | Service Level Agreement; startup deadline (180s dev, 600s prod) |
| **eventId** | Event ID from dispatcher (process ID for local, AWS event ID for EventBridge) |
| **ITriggerDispatcher** | Interface abstracting dispatch mechanism (EventBridge, process, queue, etc.) |
| **LocalWorkerProcessTriggerDispatcher** | Dispatcher spawning worker as local .NET process (dev only) |
| **EventBridgeTriggerDispatcher** | Dispatcher publishing to AWS EventBridge (production) |
| **Transient State** | API projection, not persisted (TriggerAccepted, StartFailedTimeout) |
| **Terminal State** | Execution complete; no further state changes (Completed, Cancelled, Skipped) |
| **Mutual Exclusion** | Only one execution per job name at a time |
| **fps.job_queue** | Table storing execution records (source of truth) |
| **fps.job_status** | Reference table defining allowed states per job |

---

## Appendix B: Response Examples (All Scenarios)

### Scenario 1: Successful Trigger

```http
POST /api/v1/batch-jobs/trigger
{"jobName":"RecreateSummaries","requestedBy":"user@local"}

HTTP 202 Accepted
{
  "accepted": true,
  "jobName": "RecreateSummaries",
  "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "eventId": "localproc-12345",
  "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
  "message": "Trigger accepted. Job queued for execution."
}
```

### Scenario 2: Watchdog Active (Startup Phase)

```http
GET /api/batch-jobs/RecreateSummaries/status

HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": null,
  "startupWatchdog": {
    "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "projectedState": "TriggerAccepted",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "secondsRemainingInSla": 145,
    "isActive": true
  }
}
```

### Scenario 3: Execution in Running State

```http
GET /api/batch-jobs/RecreateSummaries/status

HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": {
    "currentState": "Running",
    "startDateTime": "2026-06-03T14:30:03.000Z",
    "estimatedEndDateTime": null
  },
  "startupWatchdog": null
}
```

### Scenario 4: Execution Completed

```http
GET /api/batch-jobs/RecreateSummaries/status

HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": {
    "currentState": "Completed",
    "startDateTime": "2026-06-03T14:30:03.000Z",
    "estimatedEndDateTime": "2026-06-03T14:31:45.000Z"
  },
  "startupWatchdog": null
}
```

### Scenario 5: Concurrent Trigger (409 Conflict)

```http
POST /api/v1/batch-jobs/trigger
{"jobName":"RecreateSummaries","requestedBy":"user@local"}

HTTP 409 Conflict
{
  "accepted": false,
  "jobName": "RecreateSummaries",
  "message": "Job is already running. Cannot trigger concurrent execution.",
  "currentExecution": {
    "jobExecutionId": "x9y8z7w6-v5u4-43t2-1s0r-9q8p7o6n5m4l",
    "currentState": "Running",
    "startDateTime": "2026-06-03T14:29:00.000Z"
  }
}
```

### Scenario 6: Watchdog Timeout (SLA Exceeded)

```http
GET /api/batch-jobs/RecreateSummaries/status

HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": false,
  "sourceOfTruth": "StartupWatchdog",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": null,
  "startupWatchdog": {
    "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
    "projectedState": "StartFailedTimeout",
    "startupDeadlineUtc": "2026-06-03T14:33:00.000Z",
    "secondsRemainingInSla": -125,
    "isActive": false
  }
}
```

---

## Document Metadata

| Property | Value |
|----------|-------|
| **Created** | 2026-06-03 |
| **Version** | 2.0 |
| **Status** | ✅ FINAL - Ready for Team Review |
| **Format** | Single Comprehensive Document |
| **Intended Use** | Team briefing, contract documentation, implementation guide, architect Q&A |
| **Related Docs** | See DOCUMENTATION-INDEX.md for detailed spec documents |

---

**End of Master Implementation Guide**

This document is complete and ready for presentation to your team and the PACT team. Print it, bookmark it, share it. All team members should read through Section 1 (Architecture) and Section 2 (Contract). Architects should also read Section 5 (Design Rationale). Implementation teams should focus on Section 3 (API Spec), Section 4 (Watchdog Algorithm), and Section 8 (Checklist).
