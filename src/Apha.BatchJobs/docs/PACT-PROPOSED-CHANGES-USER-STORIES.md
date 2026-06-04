---
title: PACT and BatchJobs Proposed Changes - User Stories
version: 1.0
status: Draft for Planning
date: 2026-06-04
owners: PACT API Team, BatchJobs Team, UI Team
---

# PACT and BatchJobs Proposed Changes - User Stories

## Purpose

This backlog converts the agreed design points into implementable user stories for planning and sprint slicing.

## Scope Guardrails

- Startup watchdog timeout (30s non-prod, 600s prod) is startup visibility only.
- Runtime timeout for long jobs is separate and job-specific.
- Retry behavior remains worker-owned and mostly internal.
- UI is render-only and must not own business rules.

## Story Format

Each story includes:
- Priority: P0 (must), P1 (high), P2 (medium)
- Scope: API, Worker, UI, or Docs
- Acceptance criteria in Given/When/Then format

## EPIC A - Reliable Run Tracking Across Sessions and Users

### US-01 - Resume Tracking After Refresh/Reopen
- Priority: P0
- Scope: UI + API
- Story:
As a user, I want my job status tracking to continue after page refresh or browser reopen, so that I do not lose visibility of the run I started.
- Acceptance Criteria:
1. Given a trigger accepted response, when the UI receives jobExecutionId and acceptedAtUtc, then it stores correlation data in browser storage.
2. Given a page refresh or reopen, when correlation data exists, then UI resumes polling the same jobExecutionId automatically.
3. Given resumed polling, when status is returned, then timeline continues from server response without client-side inferred transitions.
4. Given correlation data is missing or expired, when page loads, then UI falls back to latest job status view and shows it is not correlated to a prior run.

### US-02 - Multi-User Consistent View of Same Job
- Priority: P0
- Scope: API + UI
- Story:
As a second user opening the same job screen, I want to see consistent run state and trigger guardrails, so that concurrent users do not start duplicate runs.
- Acceptance Criteria:
1. Given a run is active, when another user opens the page, then can-run returns canRun=false and UI disables trigger.
2. Given another user has the same jobExecutionId, when they poll status, then they see the same correlated execution state.
3. Given another user does not have correlation id, when they poll by job name, then UI clearly labels the status as latest-run view.

## EPIC B - API-Owned State Semantics and Simple UI

### US-03 - UI as Thin Renderer (No Business Logic)
- Priority: P0
- Scope: UI
- Story:
As a product owner, I want the UI to only render API-provided state, so that business rules live in one place.
- Acceptance Criteria:
1. Given status responses, when UI renders, then it uses sourceOfTruth, lastExecution, and startupWatchdog only.
2. Given a transition scenario, when API changes state, then UI updates without local timer-based reclassification.
3. Given edge conditions, when API returns terminal state, then UI stops polling based on API state only.

### US-04 - Startup Watchdog Separate from Runtime Timeout
- Priority: P0
- Scope: API + Docs
- Story:
As an operator, I want startup timeout to be distinct from runtime timeout, so that long jobs are not falsely marked as failed startup.
- Acceptance Criteria:
1. Given no DB execution row yet, when startup window is active, then startupWatchdog projection is returned.
2. Given execution row appears, when status is queried, then DB state is authoritative and watchdog is null.
3. Given a long-running job already in Running, when elapsed time exceeds startup window, then status remains Running and is not converted to startup timeout.
4. Given docs are updated, when teams onboard, then they can distinguish startup watchdog from runtime timeout policy.

### US-05 - Business-Facing Canonical UI States
- Priority: P1
- Scope: UI + Docs
- Story:
As a business user, I want a simple state model, so that I can quickly understand whether the job is in progress or done.
- Acceptance Criteria:
1. Given worker internal retries, when UI renders for business users, then it does not show Retry as a separate business state.
2. Given Pending or Retry from backend internals, when business view renders, then it maps to Running/in-progress.
3. Given terminal outcomes, when UI renders, then it shows Completed, Failed, or Cancelled clearly.
4. Given startup failure projection, when configured for business view, then it is shown as Failed or Startup Timeout per agreed copy.

## EPIC C - Long Running and Operator Controls

### US-06 - Cancel Long-Running Execution
- Priority: P1
- Scope: API + Worker + UI
- Story:
As an operator, I want to cancel a long-running execution safely, so that I can stop runaway or no-longer-needed jobs.
- Acceptance Criteria:
1. Given an active execution, when user requests cancel, then API records an idempotent cancellation request for jobExecutionId.
2. Given cancellation requested, when worker reaches cancellation check points, then it exits gracefully and persists Cancelled.
3. Given cancel is already requested, when cancel is called again, then API returns alreadyRequested without side effects.
4. Given execution is terminal, when cancel is requested, then API returns a no-op response.

### US-07 - Runtime Timeout Profile Per Job
- Priority: P1
- Scope: Worker + Config + Docs
- Story:
As a platform engineer, I want per-job runtime timeout policy, so that hours-long jobs can run safely while short jobs are bounded.
- Acceptance Criteria:
1. Given job configuration, when worker executes, then timeout policy can differ by job.
2. Given RecreateSummaries expected long runtime, when configured, then it is not cut off by short default timeout.
3. Given timeout occurs, when job stops, then final state and reason are persisted and distinguishable from startup timeout.

## EPIC D - Production Resilience and Observability

### US-08 - Distributed Trigger Attempt Store in Production
- Priority: P1
- Scope: API
- Story:
As a reliability engineer, I want trigger-attempt state in distributed cache for production, so watchdog behavior is consistent across instances and restarts.
- Acceptance Criteria:
1. Given production environment, when API starts, then ITriggerAttemptStore uses Redis-backed implementation.
2. Given API restarts or load-balanced instance switch, when status is called with jobExecutionId, then watchdog context is still available within TTL.
3. Given local/dev environment, when API starts, then in-memory store remains available for local simplicity.

### US-09 - Correlation-First Status Contract
- Priority: P1
- Scope: API + UI
- Story:
As support staff, I want status polling to prefer jobExecutionId correlation, so that troubleshooting is deterministic.
- Acceptance Criteria:
1. Given jobExecutionId is provided, when status is requested, then response is for that execution id.
2. Given jobExecutionId is invalid or missing, when status is requested, then response indicates fallback mode explicitly.
3. Given UI receives fallback mode, when rendered, then user sees that the view is latest-run, not correlated-run.

### US-10 - Retry as Internal Operational Signal
- Priority: P2
- Scope: Worker + API + Docs
- Story:
As an operator, I want retry behavior visible in logs and diagnostics but not overemphasized in business UI, so we reduce confusion.
- Acceptance Criteria:
1. Given transient failure and retry attempt, when logs are emitted, then attempt number and exception classification are present.
2. Given business UI mode, when rendering, then Retry is not shown as primary state.
3. Given support/debug view, when enabled, then retry attempt details are accessible.

### US-13 - Internal Stale-Lock Reconciliation (No Public Unlock Contract)
- Priority: P0
- Scope: Worker + API + Docs
- Story:
As a platform owner, I want stale lock cleanup to be automatic and internal, so production recovery does not depend on externally called unlock endpoints.
- Acceptance Criteria:
1. Given a lock exists but execution is terminal or missing, when lock is evaluated, then stale lock is auto-released by internal logic.
2. Given production environment, when API contract is published, then manual unlock endpoint is excluded from public integration contract.
3. Given local/dev environment, when break-glass recovery is needed, then a guarded local-only path may exist for operator troubleshooting.
4. Given documentation, when reviewed by teams, then stale lock recovery is documented as system-owned behavior, not UI-owned behavior.

## EPIC E - Documentation and Contract Alignment

### US-11 - Contract and Handoff Updates
- Priority: P0
- Scope: Docs
- Story:
As delivery teams, we want handoff documents aligned with implemented behavior, so that integration work and QA expectations are accurate.
- Acceptance Criteria:
1. Given endpoint inventory, when docs are reviewed, then implemented routes and examples match code.
2. Given watchdog behavior, when docs are reviewed, then startup-only semantics and sourceOfTruth usage are clear.
3. Given cache strategy, when docs are reviewed, then local in-memory and production Redis guidance are explicit.

### US-12 - End-to-End Test Scenarios for Session Recovery and Multi-User
- Priority: P1
- Scope: QA + UI + API
- Story:
As QA, I want explicit scenarios for refresh, reopen, and second-user behavior, so regressions are caught before release.
- Acceptance Criteria:
1. Given trigger accepted, when browser refreshes, then run tracking resumes.
2. Given browser closes and reopens, when storage is available, then run tracking resumes.
3. Given second user opens screen during active run, when can-run is checked, then trigger remains disabled.
4. Given execution completes while two users are polling, when status updates, then both clients converge to same terminal state.

## Suggested Delivery Slices

### Slice 1 (P0 Foundation)
- US-01, US-02, US-03, US-04, US-11, US-13

### Slice 2 (Operator Controls)
- US-06, US-07, US-09

### Slice 3 (Hardening and QA)
- US-08, US-10, US-12

## MVP Guardrails (Avoid Over-Engineering)

Implement these first and ship:
1. Correlated resume tracking after refresh/reopen (US-01).
2. Multi-user can-run guardrail and consistent status view (US-02).
3. Thin UI with API-owned state decisions (US-03).
4. Clear startup watchdog vs runtime timeout semantics in code and docs (US-04, US-11).

Delay these unless a concrete production need appears:
1. Full cancel flow with UI controls (US-06).
2. Per-job runtime timeout policy framework beyond current config defaults (US-07).
3. Distributed trigger-attempt store in Redis (US-08) if running single API instance only.

Minimum architecture quality bar before starting implementation:
1. Every trigger has immutable jobExecutionId and is logged end-to-end.
2. Status API is correlation-first and explicit when falling back to latest-run view.
3. UI never infers state transitions with local timers.
4. Startup watchdog cannot override DB-backed Running/Completed/Failed/Cancelled.
5. Retry remains worker-internal unless a durable retry queue is introduced.
6. Stale lock cleanup is automated and treated as internal platform behavior.

## Notes from Current Analysis

- Retry exists in worker orchestration, but business value of exposing Retry on UI is low for current model.
- Full rollback flow means retried attempt can still be useful for transient infrastructure failures.
- The business-facing state set can remain concise while preserving rich diagnostics in logs.
