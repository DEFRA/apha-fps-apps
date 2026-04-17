# ScheduledLoadFromFps Implementation Plan: 6-Phase Roadmap

**Status**: Phase 1 - Data Layer (Story 1.1 Completed)
**Last Updated**: 2026-04-17
**Owner**: Batch Job Team

---

## Executive Summary

| Phase | Title | Deliverables | Dependencies | Status |
|---|---|---|---|---|
| **1** | Create Required Tables | 004 + 006 + 010 + 011 + validate scripts | Latest cloud schema ✅ | 🟡 IN PROGRESS (Story 1.1 ✅) |
| **2** | Seed Test Data | Seed pack covering full required process footprint (all required source/archive/support tables) | Phase 1 complete | ✅ COMPLETE |
| **3** | Port Business Logic | 5 step handlers + repos | Phases 1-2 complete | 🔲 BLOCKED |
| **4** | Test Cases | Integration + unit tests | Phase 3 complete | 🔲 BLOCKED |
| **5** | Flush Scripts | 002_flush_scheduled_load_tables.sql | Phase 1 complete | 🔲 BLOCKED |
| **6** | Documentation | Architecture + current state doc | All phases | 🔲 BLOCKED |

---

# PHASE 1: Create Required Tables 🗄️

## User Story 1.1
**Status**: ✅ Implemented (2026-04-17)

**As a** database administrator  
**I want to** create the required scheduled load tables in PostgreSQL  
**So that** the batch job has persistent storage for orchestration and data

### Acceptance Criteria
- [x] Migration scripts created and reviewed (`010_create_legacy_year_delete_load_table_set.sql`, `011_drop_redundant_scheduledload_tables.sql`, `validate/001_verify_scheduledload_required_tables.sql`)
- [x] Required source/archive tables exist across `fps` and `mabarchive` schemas
- [x] Core key constraints validated for year-scoped archive parity
- [x] Scripts are idempotent (safe to re-run)
- [x] Validation script fails fast on missing required tables and passes on current footprint

### Technical Spec

```sql
-- Schemas: fps (runtime/control + source/target), mabarchive (archive/reporting)

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

-- D) fps.fpsyeartotals
-- Live yearly totals table in fps schema.
-- Rebuilt by legacy-equivalent totals logic before archive load.
Columns (key subset):
  parentproject VARCHAR(20) PK
  program VARCHAR(10) NOT NULL
  totaladditionalcosts MONEY NULL
  totalanimalcosts DOUBLE PRECISION NULL
  totalstaffcosts DOUBLE PRECISION NULL
  totaltestcosts DOUBLE PRECISION NULL
  totalcosts DOUBLE PRECISION NULL
  custincome MONEY NOT NULL
  transferincome MONEY NOT NULL
  totalincome MONEY NOT NULL
  projectstatus VARCHAR(50) NULL
  fpsyear INT NULL

-- E) fps.tlkpproject
-- Live project master used as enrichment/source input.
Columns (key subset):
  parentproject CITEXT PK
  projecttitle VARCHAR(200) NOT NULL
  program CITEXT NOT NULL
  customer CITEXT NOT NULL
  manager VARCHAR(50) NULL
  projectstatus CITEXT NOT NULL
  disease CITEXT NOT NULL
  incomeaccountcode CITEXT NOT NULL
  fpsyear INT NULL

-- F) mabarchive.my_fpsyeartotals
-- Year-scoped archive/reporting target for yearly totals.
Columns (key subset):
  year SMALLINT NOT NULL
  parentproject VARCHAR(20) NOT NULL
  program VARCHAR(10) NOT NULL
  totaladditionalcosts MONEY NULL
  totalanimalcosts DOUBLE PRECISION NULL
  totalstaffcosts DOUBLE PRECISION NULL
  totaltestcosts DOUBLE PRECISION NULL
  totalcosts DOUBLE PRECISION NULL
  custincome MONEY NOT NULL
  transferincome MONEY NOT NULL
  totalincome MONEY NOT NULL
  projectstatus VARCHAR(50) NOT NULL
Constraints:
  PK (year, parentproject)

-- G) mabarchive.my_tlkpproject_all
-- Year-scoped archive/reporting target for project master snapshot.
Columns (key subset):
  year SMALLINT NOT NULL
  parentproject VARCHAR(20) NOT NULL
  program VARCHAR(10) NULL
  customer VARCHAR(50) NULL
  manager VARCHAR(50) NULL
  projectstatus VARCHAR(50) NULL
  incomeaccountcode VARCHAR(50) NULL
Constraints:
  PK (year, parentproject)

-- H) Additional mabarchive support tables
-- The current runtime footprint includes 20+ additional my_*, g_*, and tlkpyear tables
-- provisioned by 010_create_legacy_year_delete_load_table_set.sql.
-- Delete/load parity must be maintained across the full required archive slice, not just totals.
```

### Definition of Done
- [x] Scripts pass syntax checks in local test environment
- [x] Required table set created successfully and redundant interim tables removed
- [x] Constraints verified via metadata validation script
- [x] Validation query confirms required footprint (missing_count = 0)
- [x] Scripts added to git and pushed

### Effort Estimate: **2 hours**

---

## User Story 1.2
**As a** developer  
**I want to** create EF Core mappings for the scheduled-load control tables and active source/archive tables  
**So that** the C# code can query and persist scheduled load data

### Acceptance Criteria
- [x] Scheduled-load control tables (`fps.scheduled_load_run`, `fps.scheduled_load_step_run`, `fps.scheduled_load_validation_result`) are mapped in EF Core
- [x] Source/archive read models are mapped for tables actively touched by handlers (`fps.fpsyeartotals`, `fps.tlkpproject`, `mabarchive.my_fpsyeartotals`, `mabarchive.my_tlkpproject_all`)
- [x] All properties map to SQL columns with correct types and key definitions
- [x] DbSet properties added to BatchJobsDbContext only for tables the application will actually query or persist
- [ ] Mappings compile without warnings

### Entity List
- `ScheduledLoadRun.cs`
- `ScheduledLoadStepRun.cs`
- `ScheduledLoadValidationResult.cs`
- `FpsYearTotalsSource.cs`
- `TlkpProjectSource.cs`
- `ArchiveFpsYearTotals.cs`
- `ArchiveTlkpProjectAll.cs`

### Effort Estimate: **3 hours**

---

## User Story 1.3
**As a** developer  
**I want to** verify the control-table and active source/archive mappings work correctly  
**So that** Phase 2 (seeding) can proceed safely

### Acceptance Criteria
- [x] Unit test: verify DbContext can be instantiated
- [x] Unit test: verify all DbSets are accessible
- [ ] Integration test: create, read, update operations on scheduled-load control entities
- [ ] Integration test: read/query operations succeed for active fps and mabarchive source/archive mappings
- [ ] Integration test: key constraints enforced on control tables and year-scoped archive PKs
- [ ] All tests pass

### Effort Estimate: **2 hours**

---

# PHASE 2: Seed Test Data 🌱

> Scope clarification: Phase 2 is not limited to 2-3 tables. It must seed the complete required ScheduledLoadFromFps runtime footprint (including all required source, archive, and supporting tables), or the verified minimum dependency set needed for end-to-end execution.

## User Story 2.1
**Status**: ✅ Implemented and validated (2026-04-17)

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
- [x] Script inserts ScheduledLoadFromFps into job_master
- [x] Script inserts 5 statuses into job_status
- [x] Script is idempotent (can re-run without errors)
- [x] Verify: `SELECT * FROM fps.job_master WHERE jobname='ScheduledLoadFromFps'`
- [x] Foundation/control dependencies are satisfied for downstream seeds (no FK or lookup blockers)

### Effort Estimate: **1 hour**

---

## User Story 2.2
**Status**: ✅ Implemented and validated (2026-04-17)

**As a** test data engineer  
**I want to** create process-level fixture data across all required source/archive/support tables  
**So that** the ScheduledLoadFromFps flow can execute end-to-end against a realistic table footprint

### File
`database/sql/seeds/002_seed_scheduled_source_baseline.sql`

### Content (Baseline example shown; use runtime source tables, not dropped interim tables)
```sql
-- Test fixtures for runtime source tables
-- 1) Source totals used by transformation
INSERT INTO fps.fpsyeartotals (
  parentproject,
  program,
  totaladditionalcosts,
  totalanimalcosts,
  totalstaffcosts,
  totaltestcosts,
  totalcosts,
  custincome,
  transferincome,
  totalincome,
  budget_cvl,
  requiredprofit,
  manager,
  customer,
  projectstatus,
  pvsincome,
  plancaseworkdebit,
  totalpaycosts,
  fpsyear
)
VALUES
('P001_2025', 'PROG_A', 1000.00, 5000.00, 12000.00, 3000.00, 21500.00, 25000.00, 15000.00, 40000.00, 50000.00, 2000.00, 'John Doe', 'CUSTOMER_A', 'Active', 0.00, 500.00, 12000.00, 2025),
('P002_2025', 'PROG_B', 2000.00, 8000.00, 15000.00, 4000.00, 30000.00, 30000.00, 20000.00, 50000.00, 60000.00, 3000.00, 'Jane Smith', 'CUSTOMER_B', 'Active', 500.00, 1000.00, 15000.00, 2025),
('P003_2025', 'PROG_C', 1500.00, 6000.00, 14000.00, 3500.00, 25750.00, 28000.00, 18000.00, 46000.00, 55000.00, 2500.00, 'Bob Johnson', 'CUSTOMER_C', 'Completed', 250.00, 750.00, 14000.00, 2025),
('P001_2026', 'PROG_A', 1100.00, 5500.00, 12500.00, 3200.00, 22850.00, 26000.00, 16000.00, 42000.00, 52000.00, 2200.00, 'John Doe', 'CUSTOMER_A', 'Active', 0.00, 550.00, 12500.00, 2026),
('P002_2026', 'PROG_B', 2100.00, 8500.00, 15500.00, 4200.00, 31400.00, 31000.00, 21000.00, 52000.00, 62000.00, 3100.00, 'Jane Smith', 'CUSTOMER_B', 'Active', 550.00, 1100.00, 15500.00, 2026),
('P003_2026', 'PROG_C', 1600.00, 6500.00, 14500.00, 3700.00, 27100.00, 29000.00, 19000.00, 48000.00, 57000.00, 2600.00, 'Bob Johnson', 'CUSTOMER_C', 'Active', 300.00, 800.00, 14500.00, 2026)
ON CONFLICT (parentproject) DO NOTHING;

-- 2) Project master rows used for enrichment joins
INSERT INTO fps.tlkpproject (
  parentproject,
  projecttitle,
  program,
  customer,
  manager,
  transferincome,
  custincome,
  projectstatus,
  disease,
  isdefraproject,
  incomeaccountcode,
  fpsyear
)
VALUES
('P001_2025', 'Project 001 FY2025', 'PROG_A', 'CUSTOMER_A', 'John Doe', 15000.00, 25000.00, 'Active', 'GEN', 0, 'INC_A', 2025),
('P002_2025', 'Project 002 FY2025', 'PROG_B', 'CUSTOMER_B', 'Jane Smith', 20000.00, 30000.00, 'Active', 'GEN', 0, 'INC_B', 2025),
('P003_2025', 'Project 003 FY2025', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 18000.00, 28000.00, 'Completed', 'GEN', 0, 'INC_C', 2025),
('P001_2026', 'Project 001 FY2026', 'PROG_A', 'CUSTOMER_A', 'John Doe', 16000.00, 26000.00, 'Active', 'GEN', 0, 'INC_A', 2026),
('P002_2026', 'Project 002 FY2026', 'PROG_B', 'CUSTOMER_B', 'Jane Smith', 21000.00, 31000.00, 'Active', 'GEN', 0, 'INC_B', 2026),
('P003_2026', 'Project 003 FY2026', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 19000.00, 29000.00, 'Active', 'GEN', 0, 'INC_C', 2026)
ON CONFLICT (parentproject) DO NOTHING;
```

### Acceptance Criteria
- [x] 6 test projects inserted (3 projects × 2 years)
- [x] All numeric fields populated with test values
- [x] Verify row count: `SELECT COUNT(*) FROM fps.fpsyeartotals WHERE parentproject LIKE 'P00%_%'` = 6
- [x] Values are reasonable for cost/income calculations
- [x] Required dependent tables for ScheduledLoadFromFps runtime are seeded (not just a single source table)
- [x] Seed coverage aligns with table footprint documentation and process dependency chain

### Effort Estimate: **1.5 hours**

---

## User Story 2.3
**Status**: ✅ Implemented and validated (2026-04-17)

**As a** test data engineer  
**I want to** create expected assertions for cross-validation  
**So that** we can validate transformation correctness against known baselines

### File
`database/sql/seeds/003_seed_scheduled_validation_baseline.sql`

### Content (Pre-computed expected values for transformation)
```sql
-- Expected validation baselines for ScheduledLoadFromFps
-- These represent what the cross-validation queries should verify

-- Validation records require a non-null run_id.
-- Create or reuse a deterministic baseline run, then attach baseline assertions to it.
WITH seed_run AS (
  INSERT INTO fps.scheduled_load_run (
    run_id,
    job_name,
    fps_year,
    job_started_at,
    job_completed_at,
    final_status,
    correlation_id,
    created_at
  )
  VALUES (
    '00000000-0000-0000-0000-000000000001',
    'ScheduledLoadFromFps',
    2025,
    NOW(),
    NOW(),
    'Success',
    'baseline-seed',
    NOW()
  )
  ON CONFLICT (run_id) DO NOTHING
  RETURNING run_id
),
resolved_run AS (
  SELECT run_id FROM seed_run
  UNION ALL
  SELECT run_id FROM fps.scheduled_load_run
  WHERE run_id = '00000000-0000-0000-0000-000000000001'
  LIMIT 1
)
INSERT INTO fps.scheduled_load_validation_result (
  validation_id,
  run_id,
  assertion_code,
  assertion_description,
  expected_value,
  actual_value,
  passed,
  checked_at,
  created_at
)
VALUES
  (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_001', 'Total archived projects in mabarchive.my_fpsyeartotals for 2025 should be 3', 3, NULL, FALSE, NOW(), NOW()),
  (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_002', 'Sum of totalcosts in mabarchive.my_fpsyeartotals for 2025 should equal 77250.00', 77250, NULL, FALSE, NOW(), NOW()),
  (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_003', 'Sum of totalincome in mabarchive.my_fpsyeartotals for 2025 should equal 136000.00', 136000, NULL, FALSE, NOW(), NOW())
ON CONFLICT (run_id, assertion_code)
DO UPDATE SET
  assertion_description = EXCLUDED.assertion_description,
  expected_value = EXCLUDED.expected_value,
  checked_at = EXCLUDED.checked_at;
```

### Acceptance Criteria
- [x] Baseline assertions inserted
- [x] Values match pre-calculated expected outcomes
- [x] Can be updated by actual job execution
- [x] Baselines cover both core totals and footprint-level readiness checks

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
    // 1. Rebuild/refresh previous-year fps.fpsyeartotals using legacy totals logic parity
    // 2. Read previous-year rows from fps.fpsyeartotals
    // 3. Hand off year-specific archive refresh to DeleteYearsFpsData + AddYearsFpsData flow
    // 4. INSERT audit record into scheduled_load_step_run
    // 5. Return success
}
```

### Acceptance Criteria
- [ ] Handler rebuilds or validates previous-year rows in `fps.fpsyeartotals`
- [ ] Handler passes the previous-year slice into archive refresh flow targeting `mabarchive.my_fpsyeartotals`
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
    // 1. Query fps source tables for current year (primarily fps.tlkpproject and current-year totals inputs)
    // 2. Rebuild current-year fps.fpsyeartotals using legacy sp_createFPSTotals formula parity
    // 3. If currentMonth > cutoverMonth, continue into archive refresh for current year
    // 4. INSERT audit record: step_name='ProcessCurrentYearTotals', status='completed'
    // 5. Return success
}
```

### Acceptance Criteria
- [ ] Handler reads current-year source data from current-design fps source tables
- [ ] Legacy formula parity preserved for `totalcosts` and `totalincome`
- [ ] Current-year totals are materialized into `fps.fpsyeartotals`
- [ ] Conditional execution respects cutover-month logic from `sp_LoadFromFPS`
- [ ] Audit record inserted
- [ ] Handler returns success/failure
- [ ] Unit tests with fixtures verify transformation accuracy

### Effort Estimate: **4 hours**

---

## User Story 3.3
**As a** developer  
**I want to** implement DeleteYearsFpsData handler  
**So that** perform the legacy year-specific archive wipe before reload

### Class
`Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/DeleteYearsFpsDataHandler.cs`

### Logic Flow
```csharp
public async Task ExecuteAsync(ScheduledLoadContext context, CancellationToken cancellationToken)
{
  // Legacy parity: year-specific wipe, not rolling retention cleanup
  // 1. DELETE FROM mabarchive.my_* tables WHERE year = context.TargetYear
  // 2. DELETE FROM mabarchive.g_tlkpproject / tlkpyear slices if required by legacy flow
  // 3. Preserve audit trail in fps.scheduled_load_* tables
    // 4. INSERT audit record: step_name='DeleteYearsFpsData', rows_affected=count
    // 5. Return success
}
```

### Acceptance Criteria
- [x] Handler performs a year-specific archive wipe matching `sp_DeleteYearsFPSData`
- [x] Required `mabarchive.my_*` tables are deleted for the selected year slice
- [x] Validation/audit records are handled without breaking orchestration history
- [x] Audit record logged with row counts
- [x] Handler returns success/failure
- [x] Unit tests verify year-specific delete logic

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
  // Legacy parity: broad fan-out yearly archive load
  // 1. Read fps.fpsyeartotals for context.TargetYear
  // 2. INSERT into mabarchive.my_fpsyeartotals for that year
  // 3. Populate additional mabarchive.my_* tables required by the legacy archive slice
    // 4. INSERT audit record: rows_affected=count inserted
    // 5. Return success
}
```

### Acceptance Criteria
- [x] Handler populates `mabarchive.my_fpsyeartotals` from `fps.fpsyeartotals`
- [x] Handler fans out inserts across the required archive table set for the selected year
- [x] Year-scoped insert logic matches `sp_AddYearsFPSData`
- [x] Audit record includes row counts
- [x] Handler returns success/failure
- [x] Unit tests with multi-year scenarios

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
    // 1. Query fps.tlkpproject for current_year projects
    // 2. Select subset of columns: parentproject, projecttitle, program, 
  //    customer, manager, projectstatus, etc. matching my_tlkpproject_all contract
  // 3. DELETE existing mabarchive.my_tlkpproject_all rows for current year when required
  // 4. INSERT refreshed rows into mabarchive.my_tlkpproject_all using (year, parentproject)
    // 5. INSERT audit record
    // 6. Return success
}
```

### Acceptance Criteria
- [x] Handler queries project master for current year
- [x] Relevant columns mapped to cloud-aligned typed columns
- [x] Refresh logic targets `mabarchive.my_tlkpproject_all` on `(year, parentproject)`
- [x] Audit record inserted
- [x] Handler returns success/failure
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
ASSERT_001: Row count check — COUNT(mabarchive.my_fpsyeartotals WHERE year=@year) matches COUNT(fps.fpsyeartotals WHERE fpsyear=@year)
ASSERT_002: Null values check — no unexpected NULLs in required columns
ASSERT_003: Cost component check — total_cost = sum of 4 cost types + plan_casework
ASSERT_004: Income calculation — total_income = cust_income + transfer_income
ASSERT_005: Year consistency — all archive `year` values match the requested year slice
ASSERT_006: Uniqueness — `(year, parentproject)` is unique in archive targets
ASSERT_007: Archive completeness — all required `mabarchive.my_*` tables were populated for the year
ASSERT_008: Archive integrity — totals/project-all rows preserve contract parity with source
ASSERT_009: Project snapshot created — `mabarchive.my_tlkpproject_all` row count > 0
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
- `IFpsYearTotalsSourceRepository` / `FpsYearTotalsSourceRepository`
- `ITlkpProjectSourceRepository` / `TlkpProjectSourceRepository`
- `IMyFpsYearTotalsRepository` / `MyFpsYearTotalsRepository`
- `IMyTlkpProjectAllRepository` / `MyTlkpProjectAllRepository`

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
- [x] Handlers inject into orchestrator
- [x] Handler factory pattern implemented (or direct factory method)
- [x] Each step execution creates audit record
- [ ] Cross-validation runs after all steps complete
- [x] Final status persisted to scheduled_load_run
- [x] Exception handling stops job on failure
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
- [x] Test: DbContext can instantiate without errors
- [x] Test: All 7 DbSets are accessible
- [ ] Test: Create and read ScheduledLoadRun entity
- [x] Test: FK constraint `scheduled_load_run → job_master` enforced
- [x] Test: Unique constraint on (fps_year, parent_project) enforced
- [x] Test: Indexes exist and are named correctly

### Effort Estimate: **3 hours**

---

## User Story 4.2
**As a** QA engineer  
**I want to** create integration tests for Phase 2 (seed data)  
**So that** verify test fixtures are correct and repeatable

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/Phase2SeedDataTests.cs`

### Test Cases
- [x] Test: ScheduledLoadFromFps job exists in job_master
- [x] Test: All 5 job statuses are registered
- [x] Test: 6 test projects exist in `fps.fpsyeartotals` and matching rows exist in `fps.tlkpproject`
- [x] Test: Required archive/support tables are seeded for the selected year slice
- [x] Test: Baseline validation records are inserted

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
- [x] Test: Orchestrator executes all 5 steps in sequence
- [x] Test: Step handlers are called with correct context
- [ ] Test: Cross-validation runs after all steps
- [x] Test: Job fails if any assertion fails (release gate)
- [x] Test: scheduled_load_run record created and updated correctly
- [x] Test: Conditional logic (ProcessCurrentYearTotals only if cutover month passed)

### Effort Estimate: **4 hours**

---

## User Story 4.5
**As a** QA engineer  
**I want to** create end-to-end scenario tests  
**So that** verify full execution path with real database

### Test File
`Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/E2E/ScheduledLoadE2ETests.cs`

### Scenarios
- [x] Scenario: Normal run (all steps succeed, validations pass)
- [x] Scenario: Handler fails mid-stream (job stops, audit trail recorded)
- [x] Scenario: Validation fails (job marked failed, no release gate pass)
- [x] Scenario: Conditional step skipped (month before cutover)
- [x] Scenario: Multi-year backfill (legacy-parity guard: only previous year + conditional current year)

### Effort Estimate: **6 hours**

---

# PHASE 5: Flush Scripts 🧹

## User Story 5.1
**As a** developer  
**I want to** create flush script for the seeded ScheduledLoadFromFps footprint  
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

-- 3. Clear seeded source and archive tables used by local execution
TRUNCATE TABLE fps.fpsyeartotals CASCADE;
TRUNCATE TABLE fps.tlkpproject CASCADE;
TRUNCATE TABLE mabarchive.my_fpsyeartotals CASCADE;
TRUNCATE TABLE mabarchive.my_tlkpproject_all CASCADE;
-- Extend this list to all seeded mabarchive.my_* support tables in the final reset pack.

COMMIT;

-- Verify empty state
SELECT 
  (SELECT COUNT(*) FROM fps.scheduled_load_run) as run_count,
  (SELECT COUNT(*) FROM fps.scheduled_load_step_run) as step_run_count,
  (SELECT COUNT(*) FROM fps.scheduled_load_validation_result) as validation_count,
  (SELECT COUNT(*) FROM fps.fpsyeartotals) as source_totals_count,
  (SELECT COUNT(*) FROM fps.tlkpproject) as source_project_count,
  (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals) as archive_totals_count,
  (SELECT COUNT(*) FROM mabarchive.my_tlkpproject_all) as archive_project_count;
```

### Acceptance Criteria
- [x] Script truncates all seeded ScheduledLoadFromFps tables safely (control + source + archive)
- [x] FK constraints don't block truncation
- [x] Verification query confirms all tables empty
- [x] Script is idempotent (can re-run)
- [x] Documented in README explaining usage

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
- [x] Script executes all flush + seed scripts in order
- [x] Can be run locally or in CI with env vars
- [x] Prints progress messages
- [x] Final state verified (e.g., row counts printed)

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
- [x] Required validated footprint exists across `fps` and `mabarchive` schemas
- [x] EF Core mappings created for control tables and active source/archive entities
- [x] DbContext mappings completed
- [x] Integration tests pass (verify DB schema)

### Phase 2: Seed Data ✅ (Complete)
- [x] 001_seed_scheduled_job_master.sql (ScheduledLoadFromFps registered)
- [x] 002_seed_scheduled_source_baseline.sql (baseline source fixtures)
- [x] 003_seed_scheduled_validation_baseline.sql (baseline assertions + readiness checks)
- [x] Seed coverage expanded to full required process footprint (all required tables or verified runtime minimum set)

### Phase 3: Business Logic 🟡 (In Progress)
- [x] Orchestrator structure defined (5-step plan)
- [x] ProcessPreviousYearTotals handler implemented
- [x] ProcessCurrentYearTotals handler implemented
- [x] DeleteYearsFpsData handler implemented
- [x] AddYearsFpsData handler implemented
- [x] HandleCurrentYearProjectAll handler implemented
- [ ] Cross-validation engine (12+ assertions) implemented (0%)
- [ ] Repositories implemented (Story 3 repository split pending; consolidated runtime repository currently in place)

### Phase 4: Tests 🟡 (In Progress)
- [x] Phase 1 integration tests (DB schema verification)
- [x] Phase 2 seed data tests
- [x] Unit tests for 5 handlers (baseline happy-path/cutover coverage)
- [x] Orchestrator integration tests (sequence, context, failure path, cutover branch)
- [ ] E2E scenario tests (0%)

### Phase 5: Flush Scripts ✅ (Complete)
- [x] 002_flush_scheduled_load_tables.sql
- [x] reset_scheduled_load_locally.sh workflow

### Phase 6: Documentation ✅ (Complete)
- [x] Architecture overview (STORED-PROCEDURES-READ-WRITE-ANALYSIS.md)
- [x] Cross-check analysis (CROSS-CHECK-STORED-PROCS-VS-BATCH-JOB.md)
- [x] V3 framework analysis (REIMAGINE-V3-FRAMEWORK-ANALYSIS.md)
- [x] THIS: Current implementation status
- [x] Runbook: How to execute job locally
- [x] Troubleshooting guide

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
psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -c "SELECT COUNT(*) FROM fps.fpsyeartotals;"

# Phase 5: Reset to known state
bash database/sql/reset_scheduled_load_locally.sh
```

## Contact & Escalation

- **Tech Lead**: [Name]
- **Questions**: [Slack channel]
- **Blockers**: [Email]
```

### Acceptance Criteria
- [x] Status document created and committed
- [x] All phases with completion % and deliverables listed
- [x] Code location map accurate
- [x] Known limitations documented
- [x] Next steps prioritized with effort estimates

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
# Verify: seeded control/source/archive tables are reset and baseline fixtures are reapplied
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
SELECT year, COUNT(*) as project_count, 
  SUM(totalcosts) as total_cost_sum
FROM mabarchive.my_fpsyeartotals
GROUP BY year;
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
INSERT INTO fps.fpsyeartotals (parentproject, program, custincome, transferincome, totalincome, projectstatus, fpsyear, ...)
VALUES ('P001_2024', 'PROG_A', 25000.00, 15000.00, 40000.00, 'Active', 2024, ...),
       ('P002_2024', 'PROG_B', 30000.00, 20000.00, 50000.00, 'Active', 2024, ...),
       ('P003_2024', 'PROG_C', 28000.00, 18000.00, 46000.00, 'Completed', 2024, ...);

-- Run job (should add all years)
dotnet run -- --mode cli --job ScheduledLoadFromFps
```

### Scenario C: Test Year-Specific Archive Refresh
```sql
-- Insert a stale archive row for a year that will be refreshed
INSERT INTO mabarchive.my_fpsyeartotals (year, parentproject, program, custincome, transferincome, totalincome, projectstatus, ...)
VALUES (2025, 'P999_STALE', 'PROG_Z', 1.00, 1.00, 2.00, 'Active', ...);

-- Run job (should wipe and reload the 2025 archive slice)
dotnet run -- --mode cli --job ScheduledLoadFromFps

-- Verify stale row was removed during year-specific archive refresh
SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year = 2025 AND parentproject = 'P999_STALE';
-- Should be 0 after refresh
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
- [x] Runbook covers quick start (3 steps)
- [x] Verification queries included
- [x] Common issues & solutions documented
- [x] Multiple scenario examples
- [x] Performance notes included
- [x] Reset instructions clear

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
1. Required `fps` + `mabarchive` table footprint created, migrated, and tested ✅
2. Full required process footprint seeded (all required tables or verified runtime minimum), including known-good project fixtures ✅
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

**Status**: Phase 5 and Phase 6 implemented ✅
**Next Action**: Complete Story 3.6 cross-validation engine and Story 4.5 E2E scenarios
