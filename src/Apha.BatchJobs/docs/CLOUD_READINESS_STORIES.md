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
- Partial work during cancellation is deterministic: job records are updated before the process exits.
- Startup logs include JobName, RunId, and RunMode.
- Shutdown/cancellation logs include JobName, RunId, FailureCategory, and remaining shutdown window.
- Worker starts and stops the host lifecycle explicitly.

Implemented in:
- Apha.BatchJobs.Worker/Program.cs

## Story CR-002: Application-Level Retry Policy

Status: Implemented

Goal:
- Add explicit retry/backoff policy for transient failures at application layer (not only EF connection retries).

Retryable exceptions (transient infrastructure failures):
- Exception (base), TimeoutException, database/connectivity errors.

Non-retryable exceptions (never retried):
- OperationCanceledException, ArgumentException, InvalidOperationException, NotSupportedException, NotImplementedException.
- Rationale: config, validation, and business-rule errors are permanent and retrying wastes time.

Acceptance criteria:
- Retry policy driven by BatchJobSettings (RetryAttempts, RetryDelaySeconds).
- IsRetryable() classification is explicit in code and tested via unit test.
- Each attempt logs: attempt number, total attempts, ExceptionType, delay, and final outcome.
- Unit tests cover: retry-then-success, retry-exhaustion (transient), non-retryable immediate-fail.

## Story CR-003: Failure Classification and Exit Contract

Status: Implemented

Goal:
- Produce stable failure categories and exit codes that are automation-friendly.

Acceptance criteria:
- Exactly one machine-readable summary event emitted per run, covering all outcomes.
- Summary fields: RunId, ExecutionId, JobName, RunMode, Outcome, FailureCategory, ExitCode, TotalDurationMs.
- Summary is emitted for success, failure, skip, and cancellation — including early-exit paths.
- Exit codes are stable and deterministic.
- Only one terminal summary event emitted per run (in the finally block).

Operational exit code contract:
- 0 Success: job completed
- 1 BusinessFailure: job/runtime exception
- 2 ConfigurationError: configuration/registration/selection failure
- 3 Cancellation: host stop or cancellation requested
- 4 LockContentionSkip: another worker holds the lock
- 5 DependencyOutage: database/network dependency outage or timeout

Implemented in:
- Apha.BatchJobs.Worker/Program.cs

## Story CR-004: Degradation-Focused Test Scenarios

Status: Implemented

Goal:
- Validate behavior under dependency degradation, not only happy-path success.

Acceptance criteria:
- Integration tests fail loudly (not skip silently) when the test database is unavailable.
- Scenarios covered: DB outage, DB timeout, recovery, lock contention, stale-lock expiry.
- Retry exhaustion tested as a separate negative path.
- Timing-sensitive assertions validate retry delay and lock expiry, not only outcomes.
- CI degradation-test stage produces clear pass/fail evidence.

Future considerations:
- Chaos-style scenarios (mid-transaction disconnects).

Implemented in:
- Apha.BatchJobs.UnitTests/RepositoryIntegrationTests.cs
- .github/workflows/degradation-tests.yml

## Story CR-005: Idempotency and Re-entrancy

Status: Planned

Goal:
- Verify jobs can be safely re-run without duplicating data or side effects.

Acceptance criteria:
- Repeated execution of the same job does not duplicate output records.
- Partial failures can resume safely or fail safely without corrupting state.
- Idempotency expectations are defined per job type.
- External writes are protected against duplicate invocation.
- Test scenarios cover: restart-after-failure, retry-after-timeout, duplicate-trigger.

## Story CR-006: Observability and Correlation

Status: Planned

Goal:
- Formalise required correlation fields across all log layers so every run is queryable end-to-end.

Acceptance criteria:
- Every log line in worker, orchestrator, and repository layers includes RunId and JobName where applicable.
- ExecutionId is included once it is available.
- Correlation is preserved across retry attempts.
- Structured log fields support CloudWatch/App Insights filtering and alerting.
- Failure and retry telemetry is easy to query in operations dashboards.
- Log schema is standardised and documented.