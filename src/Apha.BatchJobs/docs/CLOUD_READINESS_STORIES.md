# Cloud Readiness Stories

This backlog tracks hardening work for graceful degradation before cloud rollout.

## Story CR-001: Graceful Shutdown Propagation

Status: Implemented

Goal:
- Ensure container stop signals (SIGTERM) are propagated to running jobs within the ECS forced-stop window.

Acceptance criteria:
- Worker uses host lifecycle cancellation token as the primary shutdown signal.
- A bounded graceful-shutdown window (25 s) is linked with host stopping so the process exits before ECS forces termination at 30 s.
- Job execution and retry delays both honour the linked cancellation token.
- All long-running operations must be cancellation-aware (DB calls, external APIs, loops).
- Partial work during cancellation is deterministic: job records are updated before the process exits.
- Startup logs include JobName, RunId, and RunMode.
- Shutdown/cancellation logs include JobName, RunId, FailureCategory, and remaining shutdown window.
- Worker starts and stops the host lifecycle explicitly.

Refinement Pointers:
- Ensure retry delay loops also respect cancellation immediately (no Task.Delay without token).
- Log remaining shutdown window only once per cancellation path (avoid log noise in loops).
- Confirm exit path always updates execution record before process exit (even on forced cancellation).
- Optionally add metric: `graceful_shutdown_completed` (true/false) for ops visibility.

Implemented in:
- Apha.BatchJobs.Worker/Program.cs

## Story CR-002: Application-Level Retry Policy

Status: Implemented

Goal:
- Add explicit retry/backoff policy for transient failures at application layer (not only EF connection retries).

Retryable exceptions (transient infrastructure failures):
- TimeoutException, explicitly enumerated infrastructure exceptions (e.g., NpgsqlException, HttpRequestException).
- Note: Avoid overly broad Exception (base) catch; prefer explicit infra exceptions.

Non-retryable exceptions (never retried):
- OperationCanceledException, ArgumentException, InvalidOperationException, NotSupportedException, NotImplementedException.
- Rationale: config, validation, and business-rule errors are permanent and retrying wastes time.

Acceptance criteria:
- Retry policy driven by BatchJobSettings (RetryAttempts, RetryDelaySeconds).
- IsRetryable() classification is explicit in code and tested via unit test.
- Each attempt logs: attempt number, total attempts, ExceptionType, delay, and final outcome.
- Retry delay includes jitter (randomized) to avoid thundering herd in ECS scale-out scenarios.
- Total retry duration is capped (not just attempt count) to prevent long-running containers.
- Unit tests cover: retry-then-success, retry-exhaustion (transient), non-retryable immediate-fail.

Refinement Pointers:
- Narrow retryable exception surface (prefer explicit infra exceptions over base Exception).
- Implement retry jitter (randomized delay) to avoid thundering herd in scale-out.
- Log retry classification decision (`IsRetryable` = true/false) for debugging.
- Cap total retry duration (not just attempts) to avoid containers exceeding container timeout.

## Story CR-003: Failure Classification and Exit Contract

Status: Implemented

Goal:
- Produce stable failure categories and exit codes that are automation-friendly.

Acceptance criteria:
- Exactly one machine-readable summary event emitted per run, covering all outcomes.
- Summary fields: RunId, ExecutionId, JobName, RunMode, Outcome, FailureCategory, ExitCode, TotalDurationMs, StartedAt, EndedAt.
- Summary includes human-readable message field (helps ops without parsing codes).
- Summary is emitted for success, failure, skip, and cancellation — including early-exit paths.
- Summary event is emitted to both structured logs and stdout (for container-level visibility).
- Exit codes are stable and deterministic (1:1 mapping with FailureCategory).
- Only one terminal summary event emitted per run (in the finally block).
- Summary is emitted even if logging pipeline partially fails (fallback logging).
- Lock skip (exit code 4) is logged as informational, not error.

Operational exit code contract:
- 0 Success: job completed
- 1 BusinessFailure: job/runtime exception
- 2 ConfigurationError: configuration/registration/selection failure
- 3 Cancellation: host stop or cancellation requested
- 4 LockContentionSkip: another worker holds the lock (informational)
- 5 DependencyOutage: database/network dependency outage or timeout

Refinement Pointers:
- Ensure summary event is emitted even if logging pipeline partially fails (fallback logging).
- Validate exit code mapping is strictly 1:1 with failure category (no overlaps).
- Lock skip (4) should NOT log as error → should be informational only.
- Add StartedAt/EndedAt timestamps for easier timeline analysis in ops dashboards.

Implemented in:
- Apha.BatchJobs.Worker/Program.cs

## Story CR-004: Degradation-Focused Test Scenarios

Status: Implemented

Goal:
- Validate behavior under dependency degradation, not only happy-path success.

Acceptance criteria:
- Integration tests fail loudly (not skip silently) when the test database is unavailable.
- Scenarios covered: DB outage, DB timeout, recovery, lock contention, stale-lock expiry.
- Retry exhaustion tested as a separate negative path with exit code assertion.
- Timing-sensitive assertions validate retry delay and lock expiry, not only outcomes.
- CI degradation-test stage is surfaced separately (not buried in logs) with explicit pass/fail.
- Structured log fields are validated in at least one integration test.
- At least one test validates correlation (RunId propagation across layers).
- Retry exhaustion scenario verifies no partial side-effects are left behind.
- All exit codes are asserted per scenario (not just behavior).

Future considerations:
- Chaos-style scenarios (mid-transaction disconnects).
- Test design allows easy extensibility for future chaos scenarios.

Implemented in:
- Apha.BatchJobs.UnitTests/RepositoryIntegrationTests.cs
- .github/workflows/degradation-tests.yml

Refinement Pointers:
- Surface degradation stage separately in CI (not buried in main logs).
- Assert final exit code correctness per scenario (not just behavior verification).
- Validate log content (structured fields) in at least one integration test.
- Verify retry exhaustion scenario leaves no partial side-effects behind.
- Design tests to be easily extendable for future chaos scenarios.

## Story CR-005: Idempotency and Re-entrancy

Status: Planned

Goal:
- Verify jobs can be safely re-run without duplicating data or side effects.

Acceptance criteria:
- Repeated execution of the same job does not duplicate output records.
- Partial failures can resume safely or fail safely without corrupting state.
- Idempotency strategy is defined per job type (e.g., Upsert, Dedup key, Checkpointing).
- Each job explicitly declares its idempotency strategy in code/documentation.
- Idempotency boundary is clearly documented (DB vs external systems).
- External writes are protected against duplicate invocation via idempotency/correlation key.
- Concurrent duplicate trigger scenario is validated (race + retry combined).
- Idempotency works even after partial commit + retry (hardest case).
- Test scenarios cover: restart-after-failure, retry-after-timeout, duplicate-trigger.

Refinement Pointers:
- Define idempotency strategy per job (not one-size-fits-all): Upsert, Dedup key, or Checkpointing.
- Ensure idempotency boundary is clearly documented (DB vs external systems).
- Add idempotency key or correlation key where needed.
- Validate concurrent duplicate trigger scenario (race + retry combined).
- Ensure idempotency works even after partial commit + retry (edge case).

## Story CR-006: Observability and Correlation

Status: Planned

Goal:
- Formalise required correlation fields across all log layers so every run is queryable end-to-end.

Acceptance criteria:
- Every log line in worker, orchestrator, and repository layers includes RunId and JobName where applicable.
- RunId is the primary query key for all operational troubleshooting.
- ExecutionId is included once it is available.
- Correlation is preserved across retry attempts, tasks, and async boundaries.
- Log schema is standardised and documented: RunId, ExecutionId, JobName, RunMode, Attempt, Status.
- All logs are structured (no plain text concatenation); queryable by simple filters (e.g., RunId-based search).
- Log level discipline enforced:
  - Info → normal flow events
  - Warning → retries and transient failures
  - Error → terminal failures only
- Structured log fields support CloudWatch/App Insights filtering and alerting.
- Dashboard queries are defined upfront (CloudWatch/App Insights examples included).
- Failure and retry telemetry is easy to query in operations dashboards.

Refinement Pointers:
- Standardize log schema across all layers: RunId, ExecutionId, JobName, RunMode, Attempt, Status.
- Ensure correlation flows across layers AND threads/tasks (async boundaries).
- Add log level discipline: Info (normal), Warning (transient), Error (terminal only).
- All logs structured (no plain text concatenation); queries should work with simple filters.
- Define dashboard queries upfront (CloudWatch/App Insights) for ops team.

---

## Cross-Cutting Refinement Pointers

These principles apply across all stories and must be validated holistically:

**Silent Paths and Observability:**
- Ensure no silent paths anywhere (everything either logs or fails explicitly).
- Every outcome must be observable: success, failure, skip, retry, and cancellation.

**Timeouts and Coordination:**
- Keep one source of truth for exit codes (centralised doc + code constant, no magic numbers).
- Ensure all timeouts (job, retry, shutdown) are aligned and non-conflicting:
  - Job timeout < Retry total duration < Shutdown window (25s) < ECS force stop (30s)
- Validate cold start + shutdown + retry interaction together (edge case prone).

**Configuration and Defaults:**
- Keep config-driven behavior but with safe defaults.
- All timeout and retry settings must have sensible fallbacks.

**Testing and Validation:**
- Validate exit code mapping is exhaustive (one entry per FailureCategory).
- Run integration tests (degradation scenarios) in CI separately from unit tests.
- Ensure ops can query any run by RunId alone (correlation is complete).
- All failure scenarios must be reproducible via test or documented scenario (no unknown failure states).