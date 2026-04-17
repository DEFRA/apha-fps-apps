# ScheduledLoadFromFps Implementation Plan: 6-Phase Roadmap

**Status**: Phase 1 - Data Layer (Ready to Start)
**Last Updated**: 2026-04-17
**Owner**: Batch Job Team

---

## Executive Summary

| Phase | Title | Deliverables | Dependencies | Status |
|---|---|---|---|---|
| **1** | Create 7 Tables | 004_scheduled_load_tables.sql | Latest cloud schema ✅ | 🟡 READY |
| **2** | Seed Test Data | 3 seed SQL files + fixtures | Phase 1 complete | 🔲 BLOCKED |
| **3** | Port Business Logic | 5 step handlers + repos | Phases 1-2 complete | 🔲 BLOCKED |
| **4** | Test Cases | Integration + unit tests | Phase 3 complete | 🔲 BLOCKED |
| **5** | Flush Scripts | 002_flush_scheduled_load_tables.sql | Phase 1 complete | 🔲 BLOCKED |
| **6** | Documentation | Architecture + current state doc | All phases | 🔲 BLOCKED |

---

# PHASE 1: Create 7 Tables 🗄️

## User Story 1.1
**As a** database administrator  
**I want to** create the 7 scheduled load tables in PostgreSQL  
**So that** the batch job has persistent storage for orchestration and data

### Acceptance Criteria
- [ ] Migration script `004_scheduled_load_tables.sql` created and reviewed
- [ ] All 7 tables exist in batch_jobs_foundation_db.operational schema
- [ ] PK, FK, unique constraints, and indexes match planning doc
- [ ] Script is idempotent (safe to re-run)
- [ ] All columns match spec from SCHEDULED-LOAD-FPS-TABLES-SEED-FLUSH-PLAN.md

### Technical Spec

```sql
-- Schema: operational

-- A) scheduled_load_run
-- Control table for batch run lifecycle
Columns:
  run_id UUID PK
  job_name VARCHAR(100) NOT NULL (FK → job_master.jobname)
  fps_year INT NOT NULL
  job_started_at TIMESTAMPTZ NOT NULL
  job_completed_at TIMESTAMPTZ NULL
  final_status VARCHAR(50) NULL (Success|Failed|Cancelled)
  correlation_id VARCHAR(64) NOT NULL
  created_at TIMESTAMPTZ DEFAULT NOW()

Indexes:
  idx_scheduled_load_run_job_fps_year (job_name, fps_year)
  idx_scheduled_load_run_correlation_id (correlation_id)

-- B) scheduled_load_step_run
-- Audit trail for each orchestration step
Columns:
  step_run_id UUID PK
  run_id UUID NOT NULL (FK → scheduled_load_run)
  step_name VARCHAR(100) NOT NULL
  step_sequence INT NOT NULL
  started_at TIMESTAMPTZ NOT NULL
  completed_at TIMESTAMPTZ NULL
  step_status VARCHAR(50) NOT NULL
  error_message VARCHAR(500) NULL
  rows_affected INT NULL
  created_at TIMESTAMPTZ DEFAULT NOW()

Indexes:
  idx_scheduled_load_step_run_run_id (run_id)
  idx_scheduled_load_step_run_status (step_status)

-- C) scheduled_load_validation_result
-- Cross-validation assertion results
Columns:
  validation_id UUID PK
  run_id UUID NOT NULL (FK → scheduled_load_run)
  assertion_code VARCHAR(50) NOT NULL (e.g., ASSERT_001, ASSERT_002)
  assertion_description VARCHAR(500) NOT NULL
  expected_value INT/DECIMAL NULL
  actual_value INT/DECIMAL NULL
  passed BOOLEAN NOT NULL
  error_message VARCHAR(500) NULL
  checked_at TIMESTAMPTZ NOT NULL
  created_at TIMESTAMPTZ DEFAULT NOW()

Indexes:
  idx_scheduled_load_validation_run_passed (run_id, passed)
  idx_scheduled_load_validation_assertion (assertion_code)

-- D) fps_source_project_year
-- Test fixture: source project data for calculation
Columns:
  source_id BIGINT PK identity
  fps_year INT NOT NULL
  parent_project VARCHAR(50) NOT NULL
  program VARCHAR(50) NULL
  total_additional_cost NUMERIC(18,2) NOT NULL DEFAULT 0
  total_animal_cost NUMERIC(18,2) NOT NULL DEFAULT 0
  total_staff_cost NUMERIC(18,2) NOT NULL DEFAULT 0
  total_test_cost NUMERIC(18,2) NOT NULL DEFAULT 0
  plan_casework_debit NUMERIC(18,2) NOT NULL DEFAULT 0
  cust_income NUMERIC(18,2) NOT NULL DEFAULT 0
  transfer_income NUMERIC(18,2) NOT NULL DEFAULT 0
  budget_cvl NUMERIC(18,2) NULL
  required_profit NUMERIC(18,2) NULL
  manager VARCHAR(100) NULL
  customer VARCHAR(100) NULL
  project_status VARCHAR(50) NULL
  pvs_income NUMERIC(18,2) NOT NULL DEFAULT 0
  total_pay_cost NUMERIC(18,2) NOT NULL DEFAULT 0
  created_at TIMESTAMPTZ DEFAULT NOW()

Indexes:
  unique(fps_year, parent_project)
  idx_fps_source_project_year_fps_year (fps_year)

-- E) fps_year_totals
-- Target: yearly totals after transformation
Columns:
  fps_year INT NOT NULL
  parent_project VARCHAR(50) NOT NULL
  program VARCHAR(50) NULL
  total_additional_cost NUMERIC(18,2) NOT NULL
  total_animal_cost NUMERIC(18,2) NOT NULL
  total_staff_cost NUMERIC(18,2) NOT NULL
  total_test_cost NUMERIC(18,2) NOT NULL
  total_cost NUMERIC(18,2) NOT NULL
  cust_income NUMERIC(18,2) NOT NULL
  transfer_income NUMERIC(18,2) NOT NULL
  total_income NUMERIC(18,2) NOT NULL
  budget_cvl NUMERIC(18,2) NULL
  required_profit NUMERIC(18,2) NULL
  manager VARCHAR(100) NULL
  customer VARCHAR(100) NULL
  project_status VARCHAR(50) NULL
  pvs_income NUMERIC(18,2) NOT NULL
  plan_casework_debit NUMERIC(18,2) NOT NULL
  total_pay_cost NUMERIC(18,2) NOT NULL
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Constraints:
  PK (fps_year, parent_project)

Indexes:
  idx_fps_year_totals_fps_year (fps_year)
  idx_fps_year_totals_updated_at (updated_at)

-- F) fps_year_archive
-- Audit: archived year data before deletion
Columns:
  archive_id UUID PK
  fps_year INT NOT NULL
  parent_project VARCHAR(50) NOT NULL
  archive_payload JSONB NOT NULL (full fps_year_totals row as JSON)
  archived_reason VARCHAR(100) NOT NULL (e.g., 'Before deletion', 'Year rotation')
  archived_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Indexes:
  idx_fps_year_archive_fps_year (fps_year)
  idx_fps_year_archive_archived_at (archived_at)

-- G) fps_project_all_current_year
-- Snapshot: current year project master
Columns:
  snapshot_id UUID PK
  fps_year INT NOT NULL
  parent_project VARCHAR(50) NOT NULL
  project_payload JSONB NOT NULL (full tlkpproject row as JSON)
  refreshed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Indexes:
  unique(fps_year, parent_project)
  idx_fps_project_all_current_year_fps_year (fps_year)
```

### Definition of Done
- [ ] Script passes syntax check in test environment
- [ ] All 7 tables created successfully
- [ ] Constraints verified with `\d+ table_name` in psql
- [ ] Indexes verified with `SELECT * FROM pg_indexes WHERE schemaname='operational'`
- [ ] Script added to git, reviewed by team

### Effort Estimate: **2 hours**

---

## User Story 1.2
**As a** developer  
**I want to** create EF Core entities for the 7 tables  
**So that** the C# code can query and persist scheduled load data

### Acceptance Criteria
- [ ] 7 entity classes created in Apha.BatchJobs.Domain/Entities/
- [ ] All properties map to SQL columns with correct types
- [ ] DbSet properties added to BatchJobsDbContext
- [ ] OnModelCreating() configures all 7 tables with explicit column mapping
- [ ] Entities compile without warnings

### Entity List
- `ScheduledLoadRun.cs`
- `ScheduledLoadStepRun.cs`
- `ScheduledLoadValidationResult.cs`
- `FpsSourceProjectYear.cs`
- `FpsYearTotals.cs`
- `FpsYearArchive.cs`
- `FpsProjectAllCurrentYear.cs`

### Effort Estimate: **3 hours**

---

## User Story 1.3
**As a** developer  
**I want to** verify the 7 tables and entities work correctly  
**So that** Phase 2 (seeding) can proceed safely

### Acceptance Criteria
- [ ] Unit test: verify DbContext can be instantiated
- [ ] Unit test: verify all DbSets are accessible
- [ ] Integration test: create, read, update operations on each entity
- [ ] Integration test: FK constraints enforced
- [ ] All tests pass

### Effort Estimate: **2 hours**

---

# PHASE 2: Seed Test Data 🌱

## User Story 2.1
**As a** test data engineer  
**I want to** create seed script for scheduled job master  
**So that** the ScheduledLoadFromFps job is registered as a known job

### File
`database/sql/seeds/001_seed_scheduled_job_master.sql`

### Content
```sql
-- Insert ScheduledLoadFromFps job definition into job_master
INSERT INTO fps.job_master (jobname, frequency, note, timetolive, created_at, updated_at)
VALUES (
  'ScheduledLoadFromFps',
  'Monthly',
  'Scheduled batch job for loading/transforming FPS data from cloud snapshot',
  3600,
  NOW(),
  NOW()
) ON CONFLICT (jobname) DO NOTHING;

-- Get the job ID for status inserts
WITH job_ids AS (
  SELECT jobid FROM fps.job_master WHERE jobname = 'ScheduledLoadFromFps'
)
-- Insert job statuses
INSERT INTO fps.job_status (jobid, status, created_at)
VALUES 
  ((SELECT jobid FROM job_ids), 'Queued', NOW()),
  ((SELECT jobid FROM job_ids), 'Running', NOW()),
  ((SELECT jobid FROM job_ids), 'Completed', NOW()),
  ((SELECT jobid FROM job_ids), 'Failed', NOW()),
  ((SELECT jobid FROM job_ids), 'Cancelled', NOW())
ON CONFLICT (jobid, status) DO NOTHING;
```

### Acceptance Criteria
- [ ] Script inserts ScheduledLoadFromFps into job_master
- [ ] Script inserts 5 statuses into job_status
- [ ] Script is idempotent (can re-run without errors)
- [ ] Verify: `SELECT * FROM fps.job_master WHERE jobname='ScheduledLoadFromFps'`

### Effort Estimate: **1 hour**

---

## User Story 2.2
**As a** test data engineer  
**I want to** create test fixture data for fps_source_project_year  
**So that** we have known-good source data for transformation testing

### File
`database/sql/seeds/002_seed_scheduled_source_baseline.sql`

### Content (Example 3 projects, 2025-2026 fiscal years)
```sql
-- Test fixtures for fps_source_project_year
INSERT INTO operational.fps_source_project_year (
  fps_year, parent_project, program,
  total_additional_cost, total_animal_cost, total_staff_cost, total_test_cost,
  plan_casework_debit, cust_income, transfer_income,
  budget_cvl, required_profit, manager, customer, project_status,
  pvs_income, total_pay_cost, created_at
)
VALUES
-- Project P001, 2025
(2025, 'P001', 'PROG_A', 1000.00, 5000.00, 12000.00, 3000.00, 500.00, 25000.00, 15000.00, 50000.00, 2000.00, 'John Doe', 'CUSTOMER_A', 'Active', 0.00, 12000.00, NOW()),
-- Project P002, 2025
(2025, 'P002', 'PROG_B', 2000.00, 8000.00, 15000.00, 4000.00, 1000.00, 30000.00, 20000.00, 60000.00, 3000.00, 'Jane Smith', 'CUSTOMER_B', 'Active', 500.00, 15000.00, NOW()),
-- Project P003, 2025
(2025, 'P003', 'PROG_C', 1500.00, 6000.00, 14000.00, 3500.00, 750.00, 28000.00, 18000.00, 55000.00, 2500.00, 'Bob Johnson', 'CUSTOMER_C', 'Completed', 250.00, 14000.00, NOW()),

-- Same projects, 2026 (next fiscal year)
(2026, 'P001', 'PROG_A', 1100.00, 5500.00, 12500.00, 3200.00, 550.00, 26000.00, 16000.00, 52000.00, 2200.00, 'John Doe', 'CUSTOMER_A', 'Active', 0.00, 12500.00, NOW()),
(2026, 'P002', 'PROG_B', 2100.00, 8500.00, 15500.00, 4200.00, 1100.00, 31000.00, 21000.00, 62000.00, 3100.00, 'Jane Smith', 'CUSTOMER_B', 'Active', 550.00, 15500.00, NOW()),
(2026, 'P003', 'PROG_C', 1600.00, 6500.00, 14500.00, 3700.00, 800.00, 29000.00, 19000.00, 57000.00, 2600.00, 'Bob Johnson', 'CUSTOMER_C', 'Active', 300.00, 14500.00, NOW())
ON CONFLICT (fps_year, parent_project) DO NOTHING;
```

### Acceptance Criteria
- [ ] 6 test projects inserted (3 projects × 2 years)
- [ ] All numeric fields populated with test values
- [ ] Verify row count: `SELECT COUNT(*) FROM operational.fps_source_project_year` = 6
- [ ] Values are reasonable for cost/income calculations

### Effort Estimate: **1.5 hours**

---

## User Story 2.3
**As a** test data engineer  
**I want to** create expected assertions for cross-validation  
**So that** we can validate transformation correctness against known baselines

### File
`database/sql/seeds/003_seed_scheduled_validation_baseline.sql`

### Content (Pre-computed expected values for transformation)
```sql
-- Expected validation baselines for ScheduledLoadFromFps
-- These represent what the cross-validation queries should verify

-- Example: Total cost calculations
-- For 2025 projects, expected sums
INSERT INTO fps.scheduled_load_validation_result (
  validation_id, run_id, assertion_code, assertion_description,
  expected_value, actual_value, passed, checked_at, created_at
)
VALUES
-- Placeholder validation record (will be updated by actual cross-validation phase)
(gen_random_uuid(), NULL, 'BASELINE_001', 'Total projects in fps_year_totals for 2025 should be 3', 
 3, NULL, FALSE, NOW(), NOW()),
(gen_random_uuid(), NULL, 'BASELINE_002', 'Sum of total_cost for 2025 projects should equal 77000.00',
 77000, NULL, FALSE, NOW(), NOW()),
(gen_random_uuid(), NULL, 'BASELINE_003', 'Sum of total_income for 2025 projects should equal 166000.00',
 166000, NULL, FALSE, NOW(), NOW())
ON CONFLICT DO NOTHING;
```

### Acceptance Criteria
- [ ] Baseline assertions inserted
- [ ] Values match pre-calculated expected outcomes
- [ ] Can be updated by actual job execution

### Effort Estimate: **1 hour**

---

# PHASE 3: Port Business Logic 💻

## Phase 3 Contract Guardrails (Mandatory)

During all handler/repository implementation in this phase, enforce the following:

- [ ] Use cloud-aligned physical column contracts in persistence (`year`, `parentproject`, `totaladditionalcosts`, etc.).
- [ ] Keep formula parity with legacy SP behavior:
  - [ ] `totalcosts = totaladditionalcosts + totalanimalcosts + totalstaffcosts + totaltestcosts + plancaseworkdebit`
  - [ ] `totalincome = custincome + transferincome`
- [ ] Preserve null/default parity with legacy CASE/COALESCE behavior.
- [ ] Keep year-scoped logic strict; no unbounded updates/deletes.
- [ ] Persist all step and validation audit records.
- [ ] If cloud snapshot parity is uncertain, log/resolve through `ASK-FROM-DBA` before merging.

## User Story 3.1
**As a** developer  
**I want to** implement ProcessPreviousYearTotals handler  
**So that** archive current year data before transformation

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/ProcessPreviousYearTotalsHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
    // 1. Query operational.fps_year_totals WHERE fps_year = context.PreviousYear
    // 2. For each row, serialize to JSON object
    // 3. INSERT into operational.fps_year_archive (archive_payload)
    // 4. DELETE FROM operational.fps_year_totals WHERE fps_year = context.PreviousYear
    // 5. INSERT audit record into scheduled_load_step_run
    //    - status='completed', rows_affected=count deleted
    // 6. Return success
}
```

### Acceptance Criteria
- [ ] Handler queries fps_year_totals for previous year
- [ ] Archive rows created with cloud-aligned typed columns and archive metadata
- [ ] Source rows deleted from fps_year_totals
- [ ] Audit record inserted to scheduled_load_step_run
- [ ] Handler returns success/failure status
- [ ] Unit tests verify each operation

### Effort Estimate: **4 hours**

---

## User Story 3.2
**As a** developer  
**I want to** implement ProcessCurrentYearTotals handler  
**So that** load and transform current year data from cloud snapshot

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/ProcessCurrentYearTotalsHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
    // 1. Query sink_raw.fps__fpsyeartotals (or fps_source_project_year if offline)
  // 2. Transform: COALESCE nulls to 0, apply legacy formula rules
    // 3. UPSERT into operational.fps_year_totals
  //    - If year+parentproject exists, UPDATE mapped columns
    //    - Else INSERT new row
    // 4. INSERT audit record: step_name='ProcessCurrentYearTotals', status='completed'
    // 5. Return success
}
```

### Acceptance Criteria
- [ ] Handler reads source data (cloud via sink or local fixture)
- [ ] NULL values coalesced to 0
- [ ] Upsert logic implemented correctly (INSERT or UPDATE based on PK)
- [ ] Legacy formula parity preserved for totalcosts and totalincome
- [ ] Persistence uses cloud-aligned physical columns (`year`, `parentproject`, etc.)
- [ ] Audit record inserted
- [ ] Handler returns success/failure
- [ ] Unit tests with fixtures verify transformation accuracy

### Effort Estimate: **4 hours**

---

## User Story 3.3
**As a** developer  
**I want to** implement DeleteYearsFpsData handler  
**So that** enforce retention policy and clean up old data

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/DeleteYearsFpsDataHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
    // Retention policy: keep current year + 2 previous years, delete older
    // 1. Calculate cutoff_year = current_year - 3
    // 2. DELETE FROM fps_year_archive WHERE fps_year < cutoff_year
    // 3. DELETE FROM scheduled_load_validation_result WHERE run_id in 
    //    (SELECT run_id FROM scheduled_load_run WHERE fps_year < cutoff_year)
    // 4. INSERT audit record: step_name='DeleteYearsFpsData', rows_affected=count
    // 5. Return success
}
```

### Acceptance Criteria
- [ ] Retention policy implemented (keep 3 most recent years)
- [ ] Archive records deleted for years outside retention window
- [ ] Validation results cleaned up accordingly
- [ ] Audit record logged with row counts
- [ ] Handler returns success/failure
- [ ] Unit tests verify retention logic

### Effort Estimate: **3 hours**

---

## User Story 3.4
**As a** developer  
**I want to** implement AddYearsFpsData handler  
**So that** support multi-year backfill scenarios

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/AddYearsFpsDataHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
    // Additive: if new fiscal year data arrives, insert without deleting current
    // 1. Query fps_source_project_year for unloaded fiscal years
    // 2. INSERT (not upsert) into fps_year_totals for each new year
    // 3. INSERT into fps_project_all_current_year for new year projects
    // 4. INSERT audit record: rows_affected=count inserted
    // 5. Return success
}
```

### Acceptance Criteria
- [ ] Handler identifies new/unloaded years
- [ ] Additive insert logic (no overwrites)
- [ ] Multiple years supported
- [ ] Audit record includes row counts
- [ ] Handler returns success/failure
- [ ] Unit tests with multi-year scenarios

### Effort Estimate: **3 hours**

---

## User Story 3.5
**As a** developer  
**I want to** implement HandleCurrentYearProjectAll handler  
**So that** snapshot current year projects for reference

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/HandleCurrentYearProjectAllHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
    // 1. Query sink_raw.fps__tlkpproject (or fps_source_project_year) 
    //    for current_year projects
    // 2. Select subset of columns: parentproject, projecttitle, program, 
  //    customer, manager, projectstatus, etc. matching my_tlkpproject_all contract
  // 3. UPSERT into fps_project_all_current_year using (year, parentproject)
  //    and typed cloud-aligned columns
    // 5. INSERT audit record
    // 6. Return success
}
```

### Acceptance Criteria
- [ ] Handler queries project master for current year
- [ ] Relevant columns mapped to cloud-aligned typed columns
- [ ] Upsert logic implemented on `(year, parentproject)`
- [ ] Audit record inserted
- [ ] Handler returns success/failure
- [ ] Unit tests verify field-level mapping parity with my_tlkpproject_all contract

### Effort Estimate: **3 hours**

---

## User Story 3.6
**As a** developer  
**I want to** implement cross-validation query pack  
**So that** verify transformation correctness against 12+ assertions

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Validation/CrossValidationEngine.cs`

### Assertions (A-L)
```
ASSERT_001: Row count check — project count in fps_year_totals matches source
ASSERT_002: Null values check — no unexpected NULLs in required columns
ASSERT_003: Cost component check — total_cost = sum of 4 cost types + plan_casework
ASSERT_004: Income calculation — total_income = cust_income + transfer_income
ASSERT_005: Year consistency — all fps_year values match context.CurrentYear
ASSERT_006: Uniqueness — (fps_year, parent_project) is unique
ASSERT_007: Data freshness — updated_at is within SLA (e.g., < 1 hour old)
ASSERT_008: Archive integrity — archived rows serialized correctly to JSON
ASSERT_009: Project snapshot created — fps_project_all_current_year row count > 0
ASSERT_010: FK referential integrity — all parent_project values exist in fixtures
ASSERT_011: Numeric range check — total_cost > 0, income >= 0
ASSERT_012: Step audit trail — 5 step_run records exist for this run_id
```

### Acceptance Criteria
- [ ] 12+ assertion queries implemented
- [ ] Results persisted to scheduled_load_validation_result
- [ ] Each assertion has pass/fail status
- [ ] Job fails if any assertion fails (release gate)
- [ ] Unit tests verify each assertion logic
- [ ] Integration tests with seed data

### Effort Estimate: **6 hours**

---

## User Story 3.7
**As a** developer  
**I want to** create repositories for data access  
**So that** handlers can query and persist scheduled load data

### Interfaces & Implementations
- `IScheduledLoadRunRepository` / `ScheduledLoadRunRepository`
- `IScheduledLoadStepRunRepository` / `ScheduledLoadStepRunRepository`
- `IScheduledLoadValidationResultRepository` / `ScheduledLoadValidationResultRepository`
- `IFpsYearTotalsRepository` / `FpsYearTotalsRepository`
- `IFpsYearArchiveRepository` / `FpsYearArchiveRepository`

### Methods Per Repository
```csharp
// IScheduledLoadRunRepository
Task<ScheduledLoadRun> CreateAsync(ScheduledLoadRun run, CancellationToken ct);
Task<ScheduledLoadRun> GetByRunIdAsync(Guid runId, CancellationToken ct);
Task<ScheduledLoadRun> UpdateAsync(ScheduledLoadRun run, CancellationToken ct);
Task<List<ScheduledLoadRun>> GetByYearAsync(int fpsYear, CancellationToken ct);

// Similar methods for other repositories
```

### Acceptance Criteria
- [ ] All 5 repositories implemented with CRUD operations
- [ ] Methods use async/await with CancellationToken
- [ ] Queries use EF Core with proper includes (FK loads)
- [ ] Unit tests mock DbContext
- [ ] Integration tests use real database

### Effort Estimate: **4 hours**

---

## User Story 3.8
**As a** developer  
**I want to** wire handlers into ScheduledLoadFromFpsJobHandler  
**So that** the orchestrator executes each handler in sequence

### File
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsJobHandler.cs`

### Changes
```csharp
public async Task ExecuteAsync(CancellationToken cancellationToken = default)
{
    var plan = _planBuilder.Build();
    var runId = Guid.NewGuid(); // Create run context
    
    // Create scheduled_load_run record
    var loadRun = new ScheduledLoadRun { ... };
    await _scheduledLoadRunRepo.CreateAsync(loadRun, cancellationToken);
    
    foreach (var step in plan.Steps)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var handler = _handlerFactory.GetHandler(step);
        var result = await handler.ExecuteAsync(plan.Context, cancellationToken);
        
        // Insert step audit record
        await _stepRunRepo.CreateAsync(new ScheduledLoadStepRun { ... }, cancellationToken);
        
        if (!result.Success) throw new StepExecutionException(...);
    }
    
    // Run cross-validation
    var validationResults = await _validationEngine.ExecuteAsync(loadRun, cancellationToken);
    
    // Update scheduled_load_run final status
    loadRun.FinalStatus = validationResults.AllPassed ? "Success" : "Failed";
    await _scheduledLoadRunRepo.UpdateAsync(loadRun, cancellationToken);
}
```

### Acceptance Criteria
- [ ] Handlers inject into orchestrator
- [ ] Handler factory pattern implemented (or direct factory method)
- [ ] Each step execution creates audit record
- [ ] Cross-validation runs after all steps complete
- [ ] Final status persisted to scheduled_load_run
- [ ] Exception handling stops job on failure
- [ ] Contract guardrails are enforced and validated in tests before merge

### Effort Estimate: **3 hours**

---

# PHASE 4: Test Cases 🧪

## User Story 4.1
**As a** QA engineer  
**I want to** create integration tests for Phase 1 (table creation & mapping)  
**So that** verify database schema is correct

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/Phase1TablesTests.cs`

### Test Cases
- [ ] Test: DbContext can instantiate without errors
- [ ] Test: All 7 DbSets are accessible
- [ ] Test: Create and read ScheduledLoadRun entity
- [ ] Test: FK constraint `scheduled_load_run → job_master` enforced
- [ ] Test: Unique constraint on (fps_year, parent_project) enforced
- [ ] Test: Indexes exist and are named correctly

### Effort Estimate: **3 hours**

---

## User Story 4.2
**As a** QA engineer  
**I want to** create integration tests for Phase 2 (seed data)  
**So that** verify test fixtures are correct and repeatable

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/Phase2SeedDataTests.cs`

### Test Cases
- [ ] Test: ScheduledLoadFromFps job exists in job_master
- [ ] Test: All 5 job statuses are registered
- [ ] Test: 6 test projects exist in fps_source_project_year
- [ ] Test: Data values are reasonable (e.g., costs > 0)
- [ ] Test: Baseline validation records are inserted

### Effort Estimate: **2 hours**

---

## User Story 4.3
**As a** QA engineer  
**I want to** create unit tests for each handler  
**So that** verify business logic is correct in isolation

### Test Files
```
Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/Handlers/
  ProcessPreviousYearTotalsHandlerTests.cs
  ProcessCurrentYearTotalsHandlerTests.cs
  DeleteYearsFpsDataHandlerTests.cs
  AddYearsFpsDataHandlerTests.cs
  HandleCurrentYearProjectAllHandlerTests.cs
```

### Test Pattern (per handler)
```csharp
[Fact]
public async Task Execute_WithValidData_CreatesAuditRecord()
{
    // Arrange
    var mockRepo = new Mock<IScheduledLoadStepRunRepository>();
    var handler = new ProcessPreviousYearTotalsHandler(mockRepo);
    
    // Act
    var result = await handler.ExecuteAsync(context, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
    mockRepo.Verify(r => r.CreateAsync(It.IsAny<ScheduledLoadStepRun>(), ...));
}
```

### Test Cases Per Handler
- Happy path (valid data)
- Edge cases (empty result sets, NULL values)
- Error handling (DB exceptions)
- Audit trail creation
- Return status correctness

### Effort Estimate: **8 hours (1.5 hrs × 5 handlers + 0.5 hrs shared setup)**

---

## User Story 4.4
**As a** QA engineer  
**I want to** create integration tests for orchestrator wiring  
**So that** verify step sequence and cross-validation execution

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/ScheduledLoadOrchestrationTests.cs`

### Test Cases
- [ ] Test: Orchestrator executes all 5 steps in sequence
- [ ] Test: Step handlers are called with correct context
- [ ] Test: Cross-validation runs after all steps
- [ ] Test: Job fails if any assertion fails (release gate)
- [ ] Test: scheduled_load_run record created and updated correctly
- [ ] Test: Conditional logic (ProcessCurrentYearTotals only if cutover month passed)

### Effort Estimate: **4 hours**

---

## User Story 4.5
**As a** QA engineer  
**I want to** create end-to-end scenario tests  
**So that** verify full execution path with real database

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/E2E/ScheduledLoadE2ETests.cs`

### Scenarios
- [ ] Scenario: Normal run (all steps succeed, validations pass)
- [ ] Scenario: Handler fails mid-stream (job stops, audit trail recorded)
- [ ] Scenario: Validation fails (job marked failed, no release gate pass)
- [ ] Scenario: Conditional step skipped (month before cutover)
- [ ] Scenario: Multi-year backfill (AddYearsFpsData adds 3 years)

### Effort Estimate: **6 hours**

---

# PHASE 5: Flush Scripts 🧹

## User Story 5.1
**As a** developer  
**I want to** create flush script for scheduled load tables  
**So that** we can safely reset data during local testing and iteration

### File
`database/sql/flush/002_flush_scheduled_load_tables.sql`

### Content
```sql
-- Safe truncation in FK dependency order
-- Prevents "cannot truncate table because other tables reference it" errors

BEGIN;

-- 1. Truncate child tables first (FK references go one direction)
TRUNCATE TABLE fps.scheduled_load_validation_result CASCADE;
TRUNCATE TABLE fps.scheduled_load_step_run CASCADE;

-- 2. Then parent table
TRUNCATE TABLE fps.scheduled_load_run CASCADE;

-- 3. Clear test fixtures
TRUNCATE TABLE operational.fps_source_project_year CASCADE;
TRUNCATE TABLE operational.fps_year_totals CASCADE;
TRUNCATE TABLE operational.fps_year_archive CASCADE;
TRUNCATE TABLE operational.fps_project_all_current_year CASCADE;

-- Reset sequences (identity columns)
ALTER SEQUENCE operational.fps_source_project_year_source_id_seq RESTART WITH 1;

COMMIT;

-- Verify empty state
SELECT 
  (SELECT COUNT(*) FROM fps.scheduled_load_run) as run_count,
  (SELECT COUNT(*) FROM fps.scheduled_load_step_run) as step_run_count,
  (SELECT COUNT(*) FROM fps.scheduled_load_validation_result) as validation_count,
  (SELECT COUNT(*) FROM operational.fps_source_project_year) as source_count,
  (SELECT COUNT(*) FROM operational.fps_year_totals) as totals_count,
  (SELECT COUNT(*) FROM operational.fps_year_archive) as archive_count,
  (SELECT COUNT(*) FROM operational.fps_project_all_current_year) as project_count;
```

### Acceptance Criteria
- [ ] Script truncates all 7 tables safely (with CASCADE)
- [ ] Script resets identity sequences
- [ ] FK constraints don't block truncation
- [ ] Verification query confirms all tables empty
- [ ] Script is idempotent (can re-run)
- [ ] Documented in README explaining usage

### Effort Estimate: **1.5 hours**

---

## User Story 5.2
**As a** developer  
**I want to** create full reset workflow script  
**So that** teams can easily reset to known state

### File
`database/sql/reset_scheduled_load_locally.sh`

### Content
```bash
#!/bin/bash
# Reset scheduled load tables + reseed fixtures
# Usage: bash database/sql/reset_scheduled_load_locally.sh

set -e

DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_NAME=batch_jobs_foundation_db
DB_USER=postgres

echo "🧹 Flushing scheduled load tables..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/sql/flush/002_flush_scheduled_load_tables.sql

echo "🌱 Seeding base job master..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/sql/seeds/001_seed_scheduled_job_master.sql

echo "🌱 Seeding test fixtures..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/sql/seeds/002_seed_scheduled_source_baseline.sql

echo "🌱 Seeding validation baseline..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f database/sql/seeds/003_seed_scheduled_validation_baseline.sql

echo "✅ Reset complete! All tables flushed and reseeded."
```

### Acceptance Criteria
- [ ] Script executes all flush + seed scripts in order
- [ ] Can be run locally or in CI with env vars
- [ ] Prints progress messages
- [ ] Final state verified (e.g., row counts printed)

### Effort Estimate: **1 hour**

---

# PHASE 6: Documentation 📖

## User Story 6.1
**As a** architect  
**I want to** document current implementation state  
**So that** the team understands what's implemented vs. what's planned

### File
`SCHEDULED-LOAD-FPS-IMPLEMENTATION-STATUS.md`

### Content (Section Breakdown)
```markdown
# ScheduledLoadFromFps Implementation Status

## Overview
- Project status: 60% complete (Phases 1-4 in progress)
- Last updated: [date]
- Team: Batch Job Team

## Phase Completion Matrix

### Phase 1: Tables ✅ (Complete)
- [x] 004_scheduled_load_tables.sql migration created
- [x] All 7 tables exist in operational schema
- [x] EF Core entities created (7 entities)
- [x] DbContext mappings completed
- [x] Integration tests pass (verify DB schema)

### Phase 2: Seed Data ✅ (Complete)
- [x] 001_seed_scheduled_job_master.sql (ScheduledLoadFromFps registered)
- [x] 002_seed_scheduled_source_baseline.sql (6 test projects)
- [x] 003_seed_scheduled_validation_baseline.sql (baseline assertions)

### Phase 3: Business Logic 🟡 (In Progress)
- [x] Orchestrator structure defined (5-step plan)
- [ ] ProcessPreviousYearTotals handler implemented (40%)
- [ ] ProcessCurrentYearTotals handler implemented (0%)
- [ ] DeleteYearsFpsData handler implemented (0%)
- [ ] AddYearsFpsData handler implemented (0%)
- [ ] HandleCurrentYearProjectAll handler implemented (0%)
- [ ] Cross-validation engine (12+ assertions) implemented (0%)
- [ ] Repositories implemented (5 repositories needed)

### Phase 4: Tests 🟡 (In Progress)
- [x] Phase 1 integration tests (DB schema verification)
- [x] Phase 2 seed data tests
- [ ] Unit tests for 5 handlers (0%)
- [ ] Orchestrator integration tests (0%)
- [ ] E2E scenario tests (0%)

### Phase 5: Flush Scripts ✅ (Complete)
- [x] 002_flush_scheduled_load_tables.sql
- [x] reset_scheduled_load_locally.sh workflow

### Phase 6: Documentation 🟡 (In Progress)
- [x] Architecture overview (STORED-PROCEDURES-READ-WRITE-ANALYSIS.md)
- [x] Cross-check analysis (CROSS-CHECK-STORED-PROCS-VS-BATCH-JOB.md)
- [x] V3 framework analysis (REIMAGINE-V3-FRAMEWORK-ANALYSIS.md)
- [ ] THIS: Current implementation status
- [ ] Runbook: How to execute job locally
- [ ] Troubleshooting guide

## Code Location Map

| Component | Location | Status |
|---|---|---|
| Plan builder | Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsPlanBuilder.cs | ✅ Complete |
| Job handler (orchestrator) | Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsJobHandler.cs | ⚠️ Skeleton only |
| Step enum | Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsStep.cs | ✅ Complete |
| Execution context | Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsExecutionContext.cs | ✅ Complete |
| EF entities | Apha.BatchJobs.Domain/Entities/{7 entity files} | ✅ Complete |
| DbContext | Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs | ✅ Updated with 7 DbSets |
| Repositories | Apha.BatchJobs.Infrastructure/Repositories/ | 🔲 To create |
| Tests | Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/ | 🟡 In progress |

## Key Dependency Map

```
ScheduledLoadFromFpsJobHandler (orchestrator)
  ├─ ScheduledLoadFromFpsPlanBuilder (builds 5-step plan)
  ├─ ProcessPreviousYearTotalsHandler (Step 1)
  │   ├─ IFpsYearTotalsRepository (query previous year)
  │   └─ IFpsYearArchiveRepository (archive to JSON)
  ├─ ProcessCurrentYearTotalsHandler (Step 2, conditional)
  │   ├─ IFpsSourceProjectYearRepository (read source)
  │   └─ IFpsYearTotalsRepository (upsert target)
  ├─ DeleteYearsFpsDataHandler (Step 3)
  │   ├─ IFpsYearArchiveRepository (cleanup)
  │   └─ IScheduledLoadValidationResultRepository (cleanup)
  ├─ AddYearsFpsDataHandler (Step 4)
  │   ├─ IFpsSourceProjectYearRepository (read multi-year source)
  │   └─ IFpsYearTotalsRepository (additive insert)
  ├─ HandleCurrentYearProjectAllHandler (Step 5)
  │   └─ IFpsProjectAllCurrentYearRepository (snapshot)
  ├─ CrossValidationEngine (execute 12+ assertions)
  │   └─ IScheduledLoadValidationResultRepository (persist results)
  └─ IScheduledLoadRunRepository (create/update run lifecycle)

## Stored Procedure Porting Status

| Original SP | Port Status | Mapped Handler |
|---|---|---|
| sp_createFPSTotals | 🟡 In progress | ProcessCurrentYearTotals |
| sp_deleteFPSTotals | 🟡 In progress | DeleteYearsFpsData (+ ProcessPreviousYearTotal archive) |
| sp_AddMY_FPSYearTotals | 🟡 In progress | AddYearsFpsData |
| sp_AddMY_MonthlyOutput, etc. | 🔲 Not started | (Future: add if needed for extended scope) |
| sp_AddYearsFPSData | 🟡 In progress | AddYearsFpsData |
| sp_DeleteYearsFPSData | 🟡 In progress | DeleteYearsFpsData |
| sp_LoadFromFPS | ✅ Architected as | Entire 5-step orchestration |

## Known Limitations & TODO

- [ ] Performance: Cross-validation engine may be slow with 1M+ rows (optimize later)
- [ ] Multi-tenant: Currently single job, extend if other scheduled jobs needed
- [ ] Observability: No external metrics exported yet (add if needed)
- [ ] Disaster recovery: No backup/restore procedures (add if required)

## Next Steps (Priority Order)

1. **Complete Phase 3 handlers** (all 5 + validation engine)
   - Estimated: 25 hours
   
2. **Complete Phase 4 tests** (all unit + integration + E2E)
   - Estimated: 20 hours
   
3. **Run full integration test** (Phase 1-4 end-to-end)
   - Estimated: 4 hours
   
4. **Performance testing** (stress test with production-like data volume)
   - Estimated: 8 hours
   
5. **UAT & documentation** (runbooks, troubleshooting)
   - Estimated: 8 hours

**Total Remaining Effort: ~65 hours**

## How to Run Tests Locally

```bash
# Phase 1: Verify table creation
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
dotnet test Apha.BatchJobs.UnitTests/bin/Debug/net8.0/*.Tests.dll --filter "ScheduledLoadFromFps"

# Phase 2: Verify seed data
psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -c "SELECT COUNT(*) FROM operational.fps_source_project_year;"

# Phase 5: Reset to known state
bash database/sql/reset_scheduled_load_locally.sh
```

## Contact & Escalation

- **Tech Lead**: [Name]
- **Questions**: [Slack channel]
- **Blockers**: [Email]
```

### Acceptance Criteria
- [ ] Status document created and committed
- [ ] All phases with completion % and deliverables listed
- [ ] Code location map accurate
- [ ] Known limitations documented
- [ ] Next steps prioritized with effort estimates

### Effort Estimate: **2 hours**

---

## User Story 6.2
**As a** team member  
**I want to** have a runbook for executing the job locally  
**So that** I can test and troubleshoot without production concerns

### File
`docs/SCHEDULED-LOAD-FPS-LOCAL-EXECUTION-RUNBOOK.md`

### Content (Sections)
```markdown
# Local Execution Runbook: ScheduledLoadFromFps

## Prerequisites
- Docker Desktop running with PostgreSQL container (batch_jobs_postgres)
- .NET 8 SDK installed
- dotnet tool: EF Core CLI (dotnet-ef)

## Quick Start (3 steps)

### Step 1: Ensure Database is Ready
```bash
docker ps | grep batch_jobs_postgres
# If not running:
docker-compose up -d postgres
```

### Step 2: Reset to Known State
```bash
bash database/sql/reset_scheduled_load_locally.sh
# Verify: all 7 tables empty, 6 projects in source fixture
```

### Step 3: Run Job
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
dotnet run --project Apha.BatchJobs.Worker -- --mode cli --job ScheduledLoadFromFps
```

## Expected Output
```
[INF] Starting CLI job execution. JobName: ScheduledLoadFromFps, CorrelationId: {GUID}
[INF] Executing structured step ProcessPreviousYearTotals
[INF] Executing structured step ProcessCurrentYearTotals
[INF] Executing structured step DeleteYearsFpsData
[INF] Executing structured step AddYearsFpsData
[INF] Executing structured step HandleCurrentYearProjectAll
[INF] Cross-validation completed: 12 assertions, 12 passed
[INF] Job execution completed with exit code: Success
```

## Verification Queries

After execution, verify data was persisted:

```sql
-- Check run was created
SELECT run_id, fps_year, final_status, created_at 
FROM fps.scheduled_load_run 
ORDER BY created_at DESC LIMIT 1;

-- Check step audit trail
SELECT step_name, step_status, rows_affected, created_at
FROM fps.scheduled_load_step_run 
ORDER BY created_at DESC LIMIT 5;

-- Check validation results
SELECT assertion_code, passed, COUNT(*) 
FROM fps.scheduled_load_validation_result 
GROUP BY assertion_code, passed;

-- Check data was transformed
SELECT fps_year, COUNT(*) as project_count, 
       SUM(total_cost) as total_cost_sum
FROM operational.fps_year_totals
GROUP BY fps_year;
```

## Troubleshooting

### Issue: "Job not found"
```
[ERR] Job not found. JobName: ScheduledLoadFromFps
```
**Solution**: Verify seed data was applied:
```bash
SELECT * FROM fps.job_master WHERE jobname='ScheduledLoadFromFps';
```

### Issue: "FK constraint violation"
```
[ERR] violates foreign key constraint "fk_scheduled_load_run_jobname"
```
**Solution**: Run seed script to register job master:
```bash
psql -h localhost -U postgres -d batch_jobs_foundation_db -f database/sql/seeds/001_seed_scheduled_job_master.sql
```

### Issue: "Validation failed: 3 assertions returned FALSE"
```
[ERR] Cross-validation failed. Expected 12 assertions to pass, got 10.
```
**Solution**: Check detailed results:
```sql
SELECT assertion_code, assertion_description, passed, error_message
FROM fps.scheduled_load_validation_result
WHERE passed = FALSE
ORDER BY created_at DESC;
```

## Common Scenarios

### Scenario A: Test with Different FPS Year
```bash
# Set environment variable to override year
export SCHEDULED_LOAD_FPS_YEAR=2024
dotnet run -- --mode cli --job ScheduledLoadFromFps
```

### Scenario B: Test Multi-Year Backfill
```sql
-- Insert additional years into source fixture
INSERT INTO operational.fps_source_project_year (fps_year, parent_project, ...)
VALUES (2024, 'P001', ...), (2024, 'P002', ...), (2024, 'P003', ...);

-- Run job (should add all years)
dotnet run -- --mode cli --job ScheduledLoadFromFps
```

### Scenario C: Test Retention Policy
```sql
-- Insert old archive records (year 2022)
INSERT INTO operational.fps_year_archive (fps_year, ...)
VALUES (2022, P001', ...);

-- Run job (should delete 2022 data per retention policy)
dotnet run -- --mode cli --job ScheduledLoadFromFps

-- Verify deletion
SELECT COUNT(*) FROM operational.fps_year_archive WHERE fps_year = 2022;
-- Should be 0
```

## Performance Notes

- Typical execution time with 6 projects: **< 1 second**
- Validation: **< 100ms** (12 assertions)
- Cross-validation bottleneck: If 1M+ projects, disable assertion #001 (row count)

## Resetting After Test

Always reset before next test run:
```bash
bash database/sql/reset_scheduled_load_locally.sh
```

Or manual reset:
```bash
psql -h localhost -U postgres -d batch_jobs_foundation_db -c "TRUNCATE TABLE fps.scheduled_load_run CASCADE;"
```
```

### Acceptance Criteria
- [ ] Runbook covers quick start (3 steps)
- [ ] Verification queries included
- [ ] Common issues & solutions documented
- [ ] Multiple scenario examples
- [ ] Performance notes included
- [ ] Reset instructions clear

### Effort Estimate: **1.5 hours**

---

## User Story 6.3
**As a** architect  
**I want to** create API/reference documentation for BatchJob framework  
**So that** future jobs can reuse the same patterns

### File
`docs/BATCH-JOB-FRAMEWORK-REFERENCE.md`

### Content (Sections)
- How to create a new IBatchJob implementation
- How to register job in DI
- How to use CorrelationService
- How to implement repositories
- Exit code contract
- Orchestration patterns (sequential, conditional, parallel)
- Testing strategy (unit, integration, E2E)

### Effort Estimate: **3 hours**

---

---

# Implementation Timeline & Effort Summary

| Phase | Effort | Status |
|---|---|---|
| **1. Create Tables** | 5 hrs | ✅ Ready to start |
| **2. Seed Data** | 3.5 hrs | ⏳ Blocked on Phase 1 |
| **3. Business Logic** | 26 hrs | ⏳ Blocked on Phases 1-2 |
| **4. Test Cases** | 20 hrs | ⏳ Blocked on Phase 3 |
| **5. Flush Scripts** | 2.5 hrs | ✅ Ready to start (parallel) |
| **6. Documentation** | 6.5 hrs | 🟡 In progress (parallel) |
| **TOTAL** | **63.5 hrs** | 👷 Work begins |

---

# Success Criteria (Definition of Done)

✅ **Project Complete When:**
1. All 7 tables created, migrated, and tested ✅
2. 6 test projects seeded with known-good data ✅
3. All 5 handlers implemented and passing unit tests ✅
4. Cross-validation engine passes 12+ assertions ✅
5. Orchestrator wires all components together ✅
6. E2E scenario tests pass (normal, error, edge cases) ✅
7. Flush & reset scripts work (repeatable testing) ✅
8. Documentation complete (status, runbook, reference) ✅
9. Code reviewed & merged to main branch ✅
10. Job executes successfully in integrated test environment ✅

---

# Risks & Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Cross-validation complexity | High | Break assertions into small, testable functions |
| Performance with large datasets | Medium | Test with production-scale data early (Phase 4) |
| Cloud schema changes | Medium | Keep latest-cloud-schema-columns.csv updated |
| Team unfamiliarity with EF Core | Medium | Pair programming on Phase 1; create code examples |
| Database transaction issues | Low | Use explicit transactions; test rollback scenarios |

---

# Notes for Team

- **Framework inheritance**: All V3 patterns already adopted; focus on business logic
- **Testing strategy**: Start with unit tests (handlers), then integration, then E2E
- **Seed data**: Keep minimal (6 projects) for fast iteration; scale in Phase 5
- **Documentation**: Write as you go; don't save for end
- **Code review**: Flag any deviations from V3 framework patterns

---

**Status**: Phase 1 Ready to Start 🚀
**Next Action**: Create migration script `004_scheduled_load_tables.sql`
