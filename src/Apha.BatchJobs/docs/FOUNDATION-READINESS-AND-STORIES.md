# BatchJobs Foundation — Readiness Assessment & Backlog Stories

**Assessment Date:** 2026-04-15  
**Last Updated:** 2026-04-15  
**Branch:** A-Foundation  
**Assessed by:** Copilot + Engineering review

---

## Readiness Score: 80 / 100

| Layer | Score | Status |
|---|---|---|
| Architecture & layering | 18 / 20 | ✅ Solid |
| Orchestrator lifecycle | 14 / 15 | ✅ Solid |
| Unit test coverage | 8 / 10 | ✅ Good |
| Docker / deployment | 8 / 10 | ✅ Good |
| EF Core / DB mapping | 6 / 10 | ⚠️ Gaps |
| Integration tests | 0 / 5 | ❌ Missing |
| Lock safety | 8 / 10 | ✅ Atomic acquire + DB uniqueness guard |
| Job extensibility | 4 / 10 | ⚠️ Manual wiring only |
| Config hygiene | 8 / 10 | ⚠️ Credential in source |
| Unused settings | 4 / 10 | ⚠️ Dead config |

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

**Affected files:** `Apha.BatchJobs.UnitTests/` (test project)

---

### ISSUE-04 — Job Registration Requires Manual Code Changes in Two Places
**Priority:** Medium  
Adding a new job requires editing both `DependencyInjection.cs` (service registration) and the `jobRegistry` dictionary. This is error-prone as the solution grows.

**Affected file:** `Apha.BatchJobs.Worker/DependencyInjection.cs`

---

### ISSUE-05 — Credentials Committed in `appsettings.json`
**Priority:** Medium  
Default `BatchJobsConnectionString` in `appsettings.json` includes a hardcoded password. Even though this is a local default, credentials committed to source are a risk.

**Affected file:** `appsettings.json`

---

### ISSUE-06 — `BatchJobSettings` Bound but Never Used
**Priority:** Low  
`BatchJobs` config section (`MaxConcurrentJobs`, `RetryAttempts`, `JobTimeout`, `RetryDelaySeconds`) is bound to `BatchJobSettings` but never read by the orchestrator or any other component.

**Affected files:** `Apha.BatchJobs.Domain/Configuration/BatchJobSettings.cs`, `Apha.BatchJobs.Worker/DependencyInjection.cs`

---

### ISSUE-07 — FluentAssertions Commercial License Warning
**Priority:** Low  
Test project emits a license warning at runtime. Requires either a commercial Xceed licence or a replacement assertion library (e.g. `Shouldly`).

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

**As a** developer merging DB-touching changes,  
**I want** integration tests for `BatchLockRepository` and `JobExecutionRepository`,  
**so that** a real Postgres instance validates that lock, execution, and log records are written and read correctly.

**Acceptance criteria:**
- [ ] New test project or test class `Apha.BatchJobs.IntegrationTests` created.
- [ ] Tests use a Postgres instance (docker-compose or testcontainers).
- [ ] `TryAcquireLockAsync` — verified lock is created and returned on first call; returns false on second call with same job name.
- [ ] `ReleaseLockAsync` — verified lock row is removed after release.
- [ ] `CreateExecutionRecordAsync` — verified row appears in `tbljobqueue` and `tbljobqueue_log`.
- [ ] `UpdateExecutionRecordAsync` — verified status and `updated_at` are updated; new log row appended.
- [ ] CI pipeline runs integration tests with the postgres service available.

---

### STORY-04 — Auto-register batch jobs via assembly scanning

**Issue:** ISSUE-04  
**Priority:** Medium  
**Estimate:** M (3–5 days)

**As a** developer adding a new batch job,  
**I want** the DI container to discover and register job handlers automatically,  
**so that** I only need to implement `IBatchJob` without modifying the registration bootstrap.

**Acceptance criteria:**
- [ ] `DependencyInjection.cs` scans for all types implementing `IBatchJob` in the Application assembly.
- [ ] Job registry is built from scanned types using `IBatchJob.Name` as the key.
- [ ] Manual `jobRegistry` dictionary and explicit `services.AddScoped<HealthCheckJobHandler>()` entries are removed.
- [ ] Existing `HealthCheckJobHandler` continues to resolve correctly.
- [ ] `ServiceCollectionSetupTests` verifies that at least all known job names resolve without error after the change.

---

### STORY-05 — Remove hardcoded credentials from `appsettings.json`

**Issue:** ISSUE-05  
**Priority:** Medium  
**Estimate:** S (1 day)

**As a** security-conscious engineer,  
**I want** no credentials stored in committed configuration files,  
**so that** accidental exposure through source control is prevented.

**Acceptance criteria:**
- [ ] `appsettings.json` `BatchJobsConnectionString` value is replaced with a placeholder (e.g. `"__REPLACE_VIA_ENV__"`).
- [ ] `appsettings.Development.json` retains a local dev default connection string (already gitignored via `.gitignore` pattern `appsettings.local.json` — extend or align).
- [ ] `README` / `LOCAL_TESTING_GUIDE.md` documents how to supply the connection string via env var for local runs.
- [ ] CI/CD supplies the value via a secret environment variable — no plaintext password in config files committed to the repo.

---

### STORY-06 — Wire `BatchJobSettings` into orchestrator or remove unused fields

**Issue:** ISSUE-06  
**Priority:** Low  
**Estimate:** S (1–2 days)

**As a** platform engineer,  
**I want** configured batch-job settings to actually influence runtime behaviour,  
**so that** operators can tune timeout and retry values without code changes.

**Acceptance criteria:**
- [ ] `JobOrchestrator` reads `BatchJobSettings.JobTimeout` to set `LockTimeoutSeconds` (replacing the hardcoded `3600`).
- [ ] `RetryAttempts` and `RetryDelaySeconds` are either wired into a retry policy (e.g. Polly) or removed from `BatchJobSettings` with a comment that they are planned for a future story.
- [ ] `MaxConcurrentJobs` is documented as out-of-scope for the current single-job-per-ECS-task model and either removed or commented.
- [ ] A unit test verifies that a non-default `JobTimeout` value flows through to the lock timeout parameter.

---

### STORY-07 — Replace or license FluentAssertions

**Issue:** ISSUE-07  
**Priority:** Low  
**Estimate:** XS (half day)

**As a** developer running the test suite,  
**I want** no licensing warnings in the test output,  
**so that** the CI log is clean and there is no commercial license risk.

**Acceptance criteria:**
- [ ] Either: a commercial Xceed licence is obtained and configured.
- [ ] Or: `FluentAssertions` is replaced with `Shouldly` (or another MIT-licensed assertion library) across all test files.
- [ ] All 10 existing tests pass after the replacement.
- [ ] No licensing warnings appear in `dotnet test` output.
