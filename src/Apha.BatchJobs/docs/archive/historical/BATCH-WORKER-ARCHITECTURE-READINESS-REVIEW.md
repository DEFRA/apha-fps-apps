---
title: Batch Worker Architecture Readiness Review
version: 1.0
status: Pre-Implementation Review
created: 2026-06-04
audience: Architecture, BatchJobs Worker, PACT API, UI teams
---

# Batch Worker Architecture Readiness Review

## 1. Goal

Validate the current design against the agreed boundaries:
- UI is render-only
- API owns trigger and status semantics
- Worker owns execution, retry, and cancellation behavior
- DB is source of truth for persistent execution outcomes

## 2. Current Boundary Fit

### 2.1 UI Boundary
- Direction is correct: UI should not infer business transitions locally.
- Keep UI state machine minimal for business users: Running, Completed, Failed, Cancelled.
- Keep support/debug metadata available in logs or technical views, not business labels.

### 2.2 API Boundary
- Trigger and status ownership is correctly centered in PACT API.
- Startup watchdog is correctly positioned as API-computed transient projection.
- Concern: local unlock-stale endpoint exists; should remain local break-glass only and excluded from production contract.

### 2.3 Worker Boundary
- Worker orchestrator correctly owns execution, retry, lock acquire/release, and final status update.
- Retry is transient and in-process, which is fine for current design and should stay worker-owned.
- Full rollback jobs can still safely retry for transient infrastructure failures.

## 3. Database Design Review (Batch-Related Tables)

## 3.1 Strengths

1. Correlation model is strong:
- jobexecutionid is unique in fps.job_queue.
- jobqueueid is internal execution identity.

2. State normalization is strong:
- fps.job_status is a reference table keyed by jobid plus status.
- fps.job_queue points to statusid and keeps durable run outcome.

3. Audit trail exists:
- fps.job_queue_log records status transitions and actor.

4. Lock model supports single-run:
- fps.job_lock with active lock row and expiry supports concurrency control.

## 3.2 Gaps and Risks

1. Retry attempts are not persisted meaningfully:
- Domain has RetryAttempts, but repository read paths currently return RetryAttempts=0.
- If support needs attempt analytics, current persistence is insufficient.

2. Status fallback ambiguity risk:
- Status endpoint can fall back to latest-by-job when correlation is absent/invalid.
- Multi-user and refresh flows require explicit response flag indicating fallback mode.

3. Lock recovery contract shape:
- System already performs stale lock cleanup in repository logic, which is good.
- Manual unlock endpoint in API can be misused if exposed as normal contract.

4. Runtime policy metadata visibility:
- Startup timeout and runtime timeout are separate concerns.
- Runtime policy is not surfaced in status payload, which can confuse operators for long jobs.

## 3.3 Non-Goals (Avoid Over-Engineering Now)

1. Do not build distributed workflows for retries yet.
2. Do not expose retry internals as a business UI state.
3. Do not add full event sourcing for job lifecycle at this stage.

## 4. Worker Orchestration Review

## 4.1 Strengths

1. Clear lifecycle:
- acquire lock
- create execution record
- execute with transient retry
- persist final status
- release lock

2. Retry classifier is explicit and conservative:
- retries only for transient infrastructure errors.
- non-retryable business/config errors fail fast.

3. Cancellation path is present:
- OperationCanceledException maps to Cancelled.

## 4.2 Gaps and Risks

1. Runtime timeout profile needs explicit governance:
- Different jobs may need very different max execution windows.
- Need policy table or config convention per job to prevent accidental short caps on long jobs.

2. Cancellation contract is incomplete end-to-end:
- Worker can honor cancellation token.
- No complete user-facing cancel request contract yet from API/UI to worker control plane.

3. Lock timeout versus real runtime:
- lock timeout must exceed worst-case valid runtime or support heartbeat/renewal.
- otherwise long valid runs can look like stale lock contention.

## 5. Recommended MVP Decisions (Architectural)

1. Keep canonical business UI states to 4:
- Running
- Completed
- Failed
- Cancelled

2. Keep Retry internal to worker for now.

3. Keep startup watchdog startup-only and never let it override DB-backed execution states.

4. Treat stale lock release as internal behavior:
- automation first
- local break-glass only
- no production external dependency

5. Make correlation-first behavior explicit in API response:
- add a response field indicating correlated versus latest fallback mode.

## 6. Implementation Readiness Checklist

You are ready to start if all items below are accepted:

1. Boundaries accepted by teams:
- UI render-only
- API semantic authority
- Worker execution authority

2. Contract alignment accepted:
- startup watchdog versus runtime timeout distinction
- business UI state simplification

3. Operational safety accepted:
- stale lock auto-recovery is system-owned
- manual unlock is local-only troubleshooting

4. Correlation quality accepted:
- immutable jobexecutionid across trigger, worker, status, logs
- fallback mode explicitly identified in status response

5. Retry policy accepted:
- worker-owned transient retry only
- no business UI retry state required

## 7. Suggested Sequence (Do First)

1. US-01, US-02, US-03, US-04, US-11, US-13
2. Add status response fallback-mode indicator
3. Add local-only warning and documentation for unlock-stale endpoint
4. Defer cancel flow and Redis store unless immediate production need

## 8. Final Recommendation

Proceed with implementation of user stories now.

The architecture is sufficiently robust for MVP if you enforce the boundary decisions above and avoid introducing user-facing complexity (Retry, advanced control planes) before clear operational need.