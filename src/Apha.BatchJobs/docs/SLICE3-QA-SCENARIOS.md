# Slice 3 QA Scenarios (US-08, US-10, US-12)

Date: 2026-06-04

## Scope

This checklist validates:
- US-08: Production uses distributed trigger-attempt store (Redis-backed).
- US-10: Retry is internal operational signal; business UI does not show Retry as primary state.
- US-12: Session recovery and multi-user convergence behavior.

## Preconditions

- PACT API running with configured trigger store.
- Sample UI running.
- RecreateSummaries route enabled.

## US-08 Validation

1. Run PACT API in Production environment with Redis configured.
2. Trigger RecreateSummaries and capture jobExecutionId from trigger response.
3. Restart PACT API process.
4. Call GET /api/batch-jobs/RecreateSummaries/status?jobExecutionId={capturedId}.
5. Verify startupWatchdog/trigger context remains available while TTL is valid.
6. In Local/Development, verify the same flow uses in-memory store and does not require Redis.

Expected:
- Production resolves trigger store as Redis-backed and survives API restart.
- Local/Development resolves trigger store as in-memory.

## US-10 Validation

1. Trigger a run where retry behavior may occur.
2. Confirm worker logs include:
   - Attempt number (Attempt=x/y)
   - Exception type
   - Exception classification (TransientRetryable or NonRetryable)
3. In Sample UI default mode, verify state pill never uses Retry as a primary state.
4. In debug view (append ?view=debug to Sample UI URL), verify diagnostics include retry details with raw state and mapped business state.

Expected:
- Retry diagnostics are visible operationally.
- Business UI presents Running/Completed/Failed/Cancelled without promoting Retry.

## US-12 Validation

### Scenario A: Refresh Resume

1. Trigger RecreateSummaries.
2. While running, refresh browser tab.
3. Verify UI resumes correlated polling for same jobExecutionId.

Expected:
- Correlated polling resumes automatically from browser storage.

### Scenario B: Reopen Resume

1. Trigger RecreateSummaries.
2. Close browser tab/window.
3. Reopen Sample UI before correlation TTL expiry.
4. Verify UI resumes same jobExecutionId.

Expected:
- Correlated polling resumes after reopen when stored correlation is still valid.

### Scenario C: Multi-user Guardrail

1. User A opens Sample UI and triggers run.
2. User B opens Sample UI for same job.
3. Verify can-run returns canRun=false for User B while run is active.
4. Verify User B sees status updates consistently for same execution when polling by correlation id.

Expected:
- Trigger remains disabled for second user during active run.
- Both users converge on same terminal state when execution completes.

### Scenario D: Non-correlated Fallback

1. Clear browser storage for correlation key.
2. Open Sample UI while a recent execution exists.
3. Verify UI loads latest-run view and labels it as not correlated.

Expected:
- UI explicitly indicates latest-run fallback mode.
