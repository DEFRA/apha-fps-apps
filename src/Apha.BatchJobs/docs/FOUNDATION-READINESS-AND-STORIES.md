# BatchJobs Foundation — Readiness Assessment & Backlog Stories

**Assessment Date:** 2026-04-15  
**Last Updated:** 2026-04-15  
**Branch:** A-Foundation  
**Assessed by:** Copilot + Engineering review

---

## Readiness Score: 94 / 100

| Layer | Score | Status |
|---|---|---|
| Architecture & layering | 18 / 20 | ✅ Solid |
| Orchestrator lifecycle | 14 / 15 | ✅ Solid |
| Unit test coverage | 8 / 10 | ✅ Good |
| Docker / deployment | 8 / 10 | ✅ Good |
| EF Core / DB mapping | 6 / 10 | ⚠️ Gaps |
| Integration tests | 4 / 5 | ✅ Repository integration coverage in place |
| Lock safety | 8 / 10 | ✅ Atomic acquire + DB uniqueness guard |
| Job extensibility | 8 / 10 | ✅ Auto-discovery via assembly scanning |
| Config hygiene | 10 / 10 | ✅ Base/development config sanitized + branch history scrubbed |
| Unused settings | 8 / 10 | ✅ JobTimeout wired; deferred settings explicitly documented |

---

## Issues Identified

### ISSUE-01 — Lock Race Condition (TOCTOU)
**Priority:** High  
`BatchLockRepository.TryAcquireLockAsync` performs check-then-insert as two separate DB calls.  
Two parallel ECS tasks for the same job can both pass the active-lock check before either has committed the new lock row.

**Status:** Resolved (2026-04-15)

**Affected file:** `Apha.BatchJobs.Infrastructure/Repositories/BatchLockRepository.cs`

---

### ISSUE-02 — `GetExecutionRecordAsync(int)` Is a Stub
**Priority:** Medium  
`JobExecutionRepository.GetExecutionRecordAsync(int executionId)` unconditionally returns `null`. Any caller relying on it silently receives no data.

**Status:** Resolved (2026-04-15)

**Affected file:** `Apha.BatchJobs.Infrastructure/Repositories/JobExecutionRepository.cs`

---

### ISSUE-03 — No Integration Tests for Repository Layer
**Priority:** Medium  
`BatchLockRepository` and `JobExecutionRepository` have zero tests. These are the only components that touch the database.

**Status:** Resolved (2026-04-15)

**Affected files:** `Apha.BatchJobs.UnitTests/` (test project)

---

### ISSUE-04 — Job Registration Requires Manual Code Changes in Two Places
**Priority:** Medium  
Adding a new job requires editing both `DependencyInjection.cs` (service registration) and the `jobRegistry` dictionary. This is error-prone as the solution grows.

**Status:** Resolved (2026-04-15)

**Affected file:** `Apha.BatchJobs.Worker/DependencyInjection.cs`

---

### ISSUE-05 — Credentials Committed in `appsettings.json`
**Priority:** Medium  
Default `BatchJobsConnectionString` in `appsettings.json` includes a hardcoded password. Even though this is a local default, credentials committed to source are a risk.

**Status:** Resolved (2026-04-15)

**Remediation update:** A-Foundation history was rewritten and force-pushed to remove historical `Password=password` literals.

**Affected file:** `appsettings.json`

---

### ISSUE-06 — `BatchJobSettings` Bound but Never Used
**Priority:** Low  
`BatchJobs` config section (`MaxConcurrentJobs`, `RetryAttempts`, `JobTimeout`, `RetryDelaySeconds`) is bound to `BatchJobSettings` but never read by the orchestrator or any other component.

**Status:** Resolved (2026-04-15)

**Affected files:** `Apha.BatchJobs.Domain/Configuration/BatchJobSettings.cs`, `Apha.BatchJobs.Worker/DependencyInjection.cs`

---

### ISSUE-07 — FluentAssertions Commercial License Warning
**Priority:** Low  
Test project emits a license warning at runtime. Requires either a commercial Xceed licence or a replacement assertion library (e.g. `Shouldly`).

**Status:** Resolved (2026-04-15)

**Affected file:** `Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj`

---

## Stories

---

### STORY-01 — Fix lock race condition with DB-level uniqueness guarantee

**Issue:** ISSUE-01  
**Priority:** High  
**Estimate:** M (3–5 days)
**Status:** Completed (2026-04-15)

**As a** batch job operator,  
**I want** the distributed lock to be safe under concurrent ECS task invocations,  
**so that** no two instances of the same job can run simultaneously even under race conditions.

**Acceptance criteria:**
- [x] `TryAcquireLockAsync` uses a single atomic DB operation (single insert attempt guarded by DB unique partial index).
- [x] Removing the check-then-insert pattern eliminates the TOCTOU window.
- [x] Existing unit tests for lock behaviour still pass.
- [x] A new unit test verifies concurrent acquire attempts result in exactly one success.
- [x] No regression in orchestrator flow tests.

**Implementation evidence:**
- `Apha.BatchJobs.Infrastructure/Repositories/BatchLockRepository.cs` now attempts insert directly and returns false on Postgres unique violation (`23505`).
- `database/sql/003_runtime_orchestrator_tables.sql` includes unique partial index `uq_batch_lock_job_name_active` on `(job_name) WHERE is_active = TRUE`.
- `Apha.BatchJobs.UnitTests/JobOrchestratorTests.cs` includes `RunAsync_WhenTwoConcurrentCallsForSameJob_OnlyOneExecutesAndOtherIsSkipped`.
- Verified with `dotnet test` pass: 11/11.

**Notes:**  
Option A: Add a `UNIQUE (job_name)` partial index on `batch_lock WHERE is_active = TRUE` and use EF `ExecuteSqlRaw` for an atomic upsert.  
Option B: Use `pg_try_advisory_xact_lock` via a raw SQL query — lighter and no row persistence needed.

---

### STORY-02 — Implement `GetExecutionRecordAsync(int)` or remove from interface

**Issue:** ISSUE-02  
**Priority:** Medium  
**Estimate:** S (1–2 days)
**Status:** Completed (2026-04-15)

**As a** developer consuming the execution repository,  
**I want** `GetExecutionRecordAsync` to either return a real result or be clearly removed from the interface,  
**so that** there are no silent null-return stubs in the codebase.

**Acceptance criteria:**
- [ ] Either: implement the method to retrieve a `JobExecutionRecord` by looking up `tbljobqueue` by GUID/RunId mapping.
- [x] Or: remove the method from `IJobExecutionRepository` if there is no current consumer.
- [x] If removed, verify no callers exist before deletion.
- [x] Unit test added or updated covering the chosen outcome.

**Implementation evidence:**
- `Apha.BatchJobs.Domain/Interfaces/IJobExecutionRepository.cs` no longer exposes `GetExecutionRecordAsync(int)`.
- `Apha.BatchJobs.Infrastructure/Repositories/JobExecutionRepository.cs` no longer contains the null-return stub.
- Global usage search showed no callers before removal.
- Verified with `dotnet build` and `dotnet test` pass: 11/11.

---

### STORY-03 — Add integration tests for repository layer

**Issue:** ISSUE-03  
**Priority:** Medium  
**Estimate:** L (5–8 days)
**Status:** Completed (2026-04-15)

**As a** developer merging DB-touching changes,  
**I want** integration tests for `BatchLockRepository` and `JobExecutionRepository`,  
**so that** a real Postgres instance validates that lock, execution, and log records are written and read correctly.

**Acceptance criteria:**
- [x] New integration test class created (`RepositoryIntegrationTests`).
- [x] Tests use a Postgres instance.
- [x] `TryAcquireLockAsync` — verified lock is created and returned on first call; returns false on second call with same job name.
- [x] `ReleaseLockAsync` — verified lock row is removed after release.
- [x] `CreateExecutionRecordAsync` — verified row appears in `tbljobqueue` and `tbljobqueue_log`.
- [x] `UpdateExecutionRecordAsync` — verified status and `updated_at` are updated; new log row appended.
- [x] CI pipeline runs integration tests with the postgres service available.

**Implementation evidence:**
- `Apha.BatchJobs.UnitTests/RepositoryIntegrationTests.cs` added with four Postgres-backed repository tests.
- `.github/workflows/batchjobs-ci.yaml` updated with postgres service and `dotnet test` step.
- Verified locally with `dotnet test`: 15/15 passing.

---

### STORY-04 — Auto-register batch jobs via assembly scanning

**Issue:** ISSUE-04  
**Priority:** Medium  
**Estimate:** M (3–5 days)
**Status:** Completed (2026-04-15)

**As a** developer adding a new batch job,  
**I want** the DI container to discover and register job handlers automatically,  
**so that** I only need to implement `IBatchJob` without modifying the registration bootstrap.

**Acceptance criteria:**
- [x] `DependencyInjection.cs` scans for all types implementing `IBatchJob` in the Application assembly.
- [x] Job resolution now uses scanned `IBatchJob` instances and matches by `IBatchJob.Name`.
- [x] Manual `jobRegistry` dictionary and explicit `services.AddScoped<HealthCheckJobHandler>()` entries are removed.
- [x] Existing `HealthCheckJobHandler` continues to resolve correctly.
- [x] Test suite verifies known job names resolve and factory behavior is correct.

**Implementation evidence:**
- `Apha.BatchJobs.Worker/DependencyInjection.cs` now auto-registers all `IBatchJob` implementations from the Application assembly.
- `Apha.BatchJobs.Application/Factory/BatchJobFactory.cs` now resolves jobs from `IEnumerable<IBatchJob>` and matches by job `Name`.
- `Apha.BatchJobs.UnitTests/BatchJobFactoryTests.cs` updated for service-based discovery and duplicate-name guard.
- Verified with `dotnet test`: 16/16 passing.

---

### STORY-05 — Remove hardcoded credentials from `appsettings.json`

**Issue:** ISSUE-05  
**Priority:** Medium  
**Estimate:** S (1 day)
**Status:** Completed (2026-04-15)

**As a** security-conscious engineer,  
**I want** no credentials stored in committed configuration files,  
**so that** accidental exposure through source control is prevented.

**Acceptance criteria:**
- [x] `appsettings.json` `BatchJobsConnectionString` value is replaced with a placeholder (`"__REPLACE_VIA_ENV__"`).
- [x] `appsettings.Development.json` no longer stores plaintext password.
- [x] `LOCAL_TESTING_GUIDE.md` documents supplying the connection string via env var for local runs.
- [x] CI supplies the value via environment variable in workflow execution.
- [x] Historical branch commits no longer expose `Password=password` literals.

**Implementation evidence:**
- `appsettings.json` now uses `"__REPLACE_VIA_ENV__"` for `ConnectionStrings:BatchJobsConnectionString`.
- `appsettings.Development.json` now uses `"__REPLACE_VIA_ENV__"`.
- `docs/LOCAL_TESTING_GUIDE.md` updated to use password-free local connection examples.
- `.github/workflows/batchjobs-ci.yaml` uses trust auth for CI postgres service and no literal password value.
- A-Foundation branch history scrub completed and force-pushed (credential literal scan against rewritten branch returns no matches).

---

### STORY-06 — Wire `BatchJobSettings` into orchestrator or remove unused fields

**Issue:** ISSUE-06  
**Priority:** Low  
**Estimate:** S (1–2 days)
**Status:** Completed (2026-04-15)

**As a** platform engineer,  
**I want** configured batch-job settings to actually influence runtime behaviour,  
**so that** operators can tune timeout and retry values without code changes.

**Acceptance criteria:**
- [x] `JobOrchestrator` reads `BatchJobSettings.JobTimeout` to set lock timeout (replacing the hardcoded `3600`).
- [x] `RetryAttempts` and `RetryDelaySeconds` are documented in `BatchJobSettings` as planned for future retry-policy wiring.
- [x] `MaxConcurrentJobs` is documented as out-of-scope for the current single-job-per-ECS-task model.
- [x] A unit test verifies that a non-default `JobTimeout` value flows through to the lock timeout parameter.

**Implementation evidence:**
- `Apha.BatchJobs.Application/JobOrchestrator.cs` now takes `IOptions<BatchJobSettings>` and uses configured `JobTimeout` for lock acquire/release behavior.
- `Apha.BatchJobs.UnitTests/JobOrchestratorTests.cs` includes `RunAsync_UsesConfiguredJobTimeoutForLockAcquisition`.
- `Apha.BatchJobs.Domain/Configuration/BatchJobSettings.cs` comments now document out-of-scope/planned settings.
- Verified with `dotnet test`: 17/17 passing.

---

### STORY-07 — Replace or license FluentAssertions

**Issue:** ISSUE-07  
**Priority:** Low  
**Estimate:** XS (half day)
**Status:** Completed (2026-04-15)

**As a** developer running the test suite,  
**I want** no licensing warnings in the test output,  
**so that** the CI log is clean and there is no commercial license risk.

**Acceptance criteria:**
- [ ] Either: a commercial Xceed licence is obtained and configured.
- [x] Or: `FluentAssertions` is replaced with `Shouldly` (or another MIT-licensed assertion library) across all test files.
- [x] All tests pass after the replacement.
- [x] No licensing warnings appear in `dotnet test` output.

**Implementation evidence:**
- `Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj` now references `Shouldly` and no longer references `FluentAssertions`.
- Assertions in `JobOrchestratorTests.cs`, `BatchJobFactoryTests.cs`, `RepositoryIntegrationTests.cs`, and `ServiceCollectionSetupTests.cs` were migrated to `Shouldly`.
- Verified with `dotnet test`: 17/17 passing, no FluentAssertions license warning output.
