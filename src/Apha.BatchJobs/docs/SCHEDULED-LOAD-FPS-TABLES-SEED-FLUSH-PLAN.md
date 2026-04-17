# ScheduledLoadFromFps: Additional Tables, Seed/Flush, and Cross-Validation Plan

## 1. Objective
Create a testable, deterministic database plan for ScheduledLoadFromFps so engineering can:
- add only the required tables first,
- seed realistic but controlled data,
- run the job logic in repeatable scenarios,
- and cross-validate outcomes with objective SQL checks.

This plan is intentionally implementation-ready but does not yet apply DDL changes.

## 2. Current Baseline (Verified)
Foundation operational tables already exist and are used by orchestrator persistence:
- fps.job_master
- fps.job_status
- fps.job_queue
- fps.job_queue_log
- fps.job_lock

Current ScheduledLoadFromFps code state:
- Job and step ordering exist.
- Step execution is currently a no-op placeholder.

## 3. Current Seed and Flush SQL Assessment

### 3.1 Seed status today
Current seed script is a template only and inserts no real data.
- Seed template: [database/sql/seeds/000_seed_template.sql](database/sql/seeds/000_seed_template.sql)

Impact:
- No test fixtures are currently created for ScheduledLoadFromFps.
- Integration tests rely on ad-hoc data creation inside test code, not reusable SQL fixtures.

### 3.2 Flush status today
Current flush script drops the entire operational schema and recreates it.
- Flush script: [database/sql/flush/001_flush_operational_schema.sql](database/sql/flush/001_flush_operational_schema.sql)

Impact:
- Full reset is simple and safe for local dev.
- Full reset also removes orchestrator tables and all newly added scheduled tables.
- After flush, apply scripts must always be rerun before tests.

### 3.3 Script orchestration status
- DB script execution model is documented in [database/README.md](database/README.md).
- Migration template exists at [database/sql/002_migration_template.sql](database/sql/002_migration_template.sql).

## 4. Additional Tables Required for ScheduledLoadFromFps

The current 5-step job flow is:
1. ProcessPreviousYearTotals
2. ProcessCurrentYearTotals (conditional)
3. DeleteYearsFpsData
4. AddYearsFpsData
5. HandleCurrentYearProjectAll

To make this flow testable and fully auditable, add the tables below.

## 4.1 Mandatory control tables (required)

### A) fps.scheduled_load_run
Purpose:
- One row per ScheduledLoadFromFps execution context.
- Captures month/year/cutover used by plan builder.

Key columns:
- run_id UUID PK
- jobqueueid UUID FK -> fps.job_queue(jobqueueid)
- current_year INT NOT NULL
- previous_year INT NOT NULL
- current_month INT NOT NULL CHECK (current_month BETWEEN 1 AND 12)
- cutover_month INT NOT NULL CHECK (cutover_month BETWEEN 1 AND 12)
- started_at TIMESTAMPTZ NOT NULL
- completed_at TIMESTAMPTZ NULL
- final_status VARCHAR(30) NOT NULL

Constraints/indexes:
- Unique(run_id)
- Index(jobqueueid)
- Index(current_year, previous_year)

### B) fps.scheduled_load_step_run
Purpose:
- One row per step attempt for each run.
- Supports retry and step-level diagnostics.

Key columns:
- step_run_id BIGINT identity PK
- run_id UUID FK -> fps.scheduled_load_run(run_id)
- step_name VARCHAR(100) NOT NULL
- step_sequence INT NOT NULL
- attempt_no INT NOT NULL DEFAULT 1
- step_status VARCHAR(30) NOT NULL
- rows_read BIGINT NULL
- rows_written BIGINT NULL
- checksum_before VARCHAR(128) NULL
- checksum_after VARCHAR(128) NULL
- started_at TIMESTAMPTZ NOT NULL
- completed_at TIMESTAMPTZ NULL
- error_text TEXT NULL

Constraints/indexes:
- Unique(run_id, step_sequence, attempt_no)
- Index(run_id, step_sequence)
- Check(step_sequence BETWEEN 1 AND 5)

### C) fps.scheduled_load_validation_result
Purpose:
- Persist SQL validation assertions for each run.
- Enables objective pass/fail evidence.

Key columns:
- validation_id BIGINT identity PK
- run_id UUID FK -> fps.scheduled_load_run(run_id)
- assertion_code VARCHAR(100) NOT NULL
- assertion_scope VARCHAR(100) NOT NULL
- expected_value TEXT NOT NULL
- actual_value TEXT NOT NULL
- passed BOOLEAN NOT NULL
- executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Constraints/indexes:
- Unique(run_id, assertion_code)
- Index(run_id, passed)

## 4.2 Mandatory data tables for deterministic testing (required)

### D) operational.fps_source_project_year
Purpose:
- Controlled source fixture for totals calculations.
- Replaces dependency on external legacy source during local tests.

Suggested columns:
- source_id BIGINT identity PK
- fps_year INT NOT NULL
- parent_project VARCHAR(50) NOT NULL
- program VARCHAR(50) NULL
- total_additional_cost NUMERIC(18,2) NOT NULL DEFAULT 0
- total_animal_cost NUMERIC(18,2) NOT NULL DEFAULT 0
- total_staff_cost NUMERIC(18,2) NOT NULL DEFAULT 0
- total_test_cost NUMERIC(18,2) NOT NULL DEFAULT 0
- plan_casework_debit NUMERIC(18,2) NOT NULL DEFAULT 0
- cust_income NUMERIC(18,2) NOT NULL DEFAULT 0
- transfer_income NUMERIC(18,2) NOT NULL DEFAULT 0
- budget_cvl NUMERIC(18,2) NULL
- required_profit NUMERIC(18,2) NULL
- manager VARCHAR(100) NULL
- customer VARCHAR(100) NULL
- project_status VARCHAR(50) NULL
- pvs_income NUMERIC(18,2) NOT NULL DEFAULT 0
- total_pay_cost NUMERIC(18,2) NOT NULL DEFAULT 0

Constraints/indexes:
- Unique(fps_year, parent_project)
- Index(fps_year)

### E) operational.fps_year_totals
Purpose:
- Target table for ProcessPreviousYearTotals and ProcessCurrentYearTotals.

Suggested columns:
- fps_year INT NOT NULL
- parent_project VARCHAR(50) NOT NULL
- program VARCHAR(50) NULL
- total_additional_cost NUMERIC(18,2) NOT NULL
- total_animal_cost NUMERIC(18,2) NOT NULL
- total_staff_cost NUMERIC(18,2) NOT NULL
- total_test_cost NUMERIC(18,2) NOT NULL
- total_cost NUMERIC(18,2) NOT NULL
- cust_income NUMERIC(18,2) NOT NULL
- transfer_income NUMERIC(18,2) NOT NULL
- total_income NUMERIC(18,2) NOT NULL
- budget_cvl NUMERIC(18,2) NULL
- required_profit NUMERIC(18,2) NULL
- manager VARCHAR(100) NULL
- customer VARCHAR(100) NULL
- project_status VARCHAR(50) NULL
- pvs_income NUMERIC(18,2) NOT NULL
- plan_casework_debit NUMERIC(18,2) NOT NULL
- total_pay_cost NUMERIC(18,2) NOT NULL
- updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Constraints/indexes:
- Primary key(fps_year, parent_project)
- Index(fps_year)

### F) operational.fps_year_archive
Purpose:
- Target table for DeleteYearsFpsData and AddYearsFpsData behavior.

Suggested columns:
- fps_year INT NOT NULL
- parent_project VARCHAR(50) NOT NULL
- archive_payload JSONB NOT NULL
- archived_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Constraints/indexes:
- Primary key(fps_year, parent_project)
- Index(fps_year)

### G) operational.fps_project_all_current_year
Purpose:
- Target for HandleCurrentYearProjectAll step.

Suggested columns:
- fps_year INT NOT NULL
- parent_project VARCHAR(50) NOT NULL
- project_payload JSONB NOT NULL
- refreshed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

Constraints/indexes:
- Primary key(fps_year, parent_project)
- Index(fps_year)

## 5. Seed Data Plan (deterministic and scenario-based)

Create versioned seed files in sequence order.

### 5.1 Seed files to add
- database/sql/seeds/001_seed_scheduled_job_master.sql
- database/sql/seeds/002_seed_scheduled_source_baseline.sql
- database/sql/seeds/003_seed_scheduled_expected_assertions.sql

### 5.2 Seed content by file

001_seed_scheduled_job_master.sql
- Ensure job_master has ScheduledLoadFromFps row.
- Ensure job_status includes Running, Completed, Failed, Cancelled, Skipped for this job.
- Use ON CONFLICT patterns only.

002_seed_scheduled_source_baseline.sql
- Insert source fixtures into fps_source_project_year for:
  - previous_year projects (minimum 3 rows)
  - current_year projects (minimum 3 rows)
- Include null-like and zero-value edge cases for parity checks.
- Include one row that validates total_cost and total_income formulas.

003_seed_scheduled_expected_assertions.sql
- Insert expected per-year/per-project totals for cross-validation into a helper table:
  - operational.fps_year_totals_expected
- Insert expected counts/checksums for archive and project-all targets.

## 6. Flush Plan

Current full reset script remains useful for clean local cycles.

Add one focused flush script for scheduled tables only:
- database/sql/flush/002_flush_scheduled_load_tables.sql

Behavior:
- TRUNCATE or DELETE only scheduled-load tables in FK-safe order.
- Keep orchestrator foundation tables unless explicit full reset is needed.

Recommended reset modes:
- Full reset: existing 001 flush + apply + seed (for clean-slate confidence)
- Scheduled-only reset: new 002 flush + seed (for faster iteration)

## 7. 100% Cross-Validation Plan

Cross-validation means each run produces measurable assertions that all pass.

## 7.1 Validation dimensions
- Structural validation: tables, PK/FK, indexes exist.
- Plan validation: selected steps match month/cutover rules.
- Data validation: computed totals exactly match expected fixture values.
- Idempotency validation: rerun does not create duplicates or drift.
- Cleanup validation: delete/add years behavior is exact for target years only.
- Audit validation: every step writes start/end status and row counters.

## 7.2 Required assertion set

Minimum assertion codes to implement and persist in scheduled_load_validation_result:
- SCHEMA_001: all scheduled tables exist
- PLAN_001: month <= cutover excludes current year totals step
- PLAN_002: month > cutover includes current year totals step
- TOTALS_001: previous_year totals rowcount equals expected
- TOTALS_002: previous_year totals checksum equals expected
- TOTALS_003: current_year totals checksum equals expected (only when included)
- DELETE_001: delete step affects only specified years
- ADD_001: add step inserts expected year rows
- PROJALL_001: current year project_all refresh count/checksum matches expected
- IDEMP_001: rerun with same context produces no net data delta
- AUDIT_001: exactly one step_run row per executed step attempt
- AUDIT_002: final status in scheduled_load_run matches orchestrator final status

## 7.3 Cross-validation execution sequence
1. Full flush and apply foundation scripts.
2. Apply scheduled table migration scripts.
3. Seed baseline and expected datasets.
4. Execute job in scenario A (month <= cutover).
5. Execute assertion SQL pack A and persist results.
6. Execute job in scenario B (month > cutover).
7. Execute assertion SQL pack B and persist results.
8. Rerun both scenarios for idempotency check.
9. Produce a single pass/fail report from scheduled_load_validation_result.

Release gate:
- 100% of assertions must pass in both scenarios and reruns.

## 8. Test Scenarios Required Before Logic Sign-off

Scenario A: pre-cutover
- Input: current_month = 3, cutover_month = 4
- Expected: ProcessCurrentYearTotals step is skipped

Scenario B: post-cutover
- Input: current_month = 6, cutover_month = 4
- Expected: ProcessCurrentYearTotals step is executed

Scenario C: idempotent rerun
- Repeat Scenario B without reseed
- Expected: no duplicate keys, no checksum drift

Scenario D: lock contention
- Parallel trigger attempt while one run is active
- Expected: one run executes, one skipped, no data corruption

## 9. Delivery Plan (documentation-first)

Phase 1
- Add migration design doc and DDL scripts for tables A-G.

Phase 2
- Add seed scripts 001-003 and scheduled-only flush script 002.

Phase 3
- Add SQL assertion packs and result persistence wiring.

Phase 4
- Wire ScheduledLoadFromFps step implementations to table contracts.

Phase 5
- Run full cross-validation and attach evidence report.

## 10. Known Gaps and Risks
- Current flush is all-or-nothing for operational schema.
- Current seed is template-only and does not support scheduled logic tests.
- Lock table script should be hardened with a unique partial active-lock index before concurrency testing at scale.

## 11. Definition of Done for this planning stage
- Additional table contract is agreed.
- Seed/flush strategy is agreed.
- Cross-validation assertions and pass criteria are agreed.
- Migration and test implementation can proceed without ambiguity.

## 12. VS Code PostgreSQL Connection Setup (A-Foundation Equivalent)

This workspace does not contain a folder literally named A-Foundation, but the equivalent local foundation database for this repo is:
- Project: Apha.BatchJobs
- Container: batch_jobs_postgres
- Database: batch_jobs_foundation_db
- Host/Port: localhost:5432

Installed extensions:
- ms-ossdata.vscode-pgsql
- ckolkman.vscode-postgres

### 12.1 Create reusable connection profile (recommended)
Use either extension (official PostgreSQL or PostgreSQL Management Tool) and create one saved connection:
- Name: A-Foundation-Local
- Host: localhost
- Port: 5432
- Database: batch_jobs_foundation_db
- User: postgres
- Password: (empty)
- SSL mode: disable/prefer non-SSL for local container

Note:
- Full profile + credential storage is managed by VS Code extension state, not repository files.
- After first save, the profile is reusable across sessions in the same VS Code environment.

### 12.2 Immediate smoke checks after connecting
Run these queries in order:

```sql
SELECT current_database() AS db, current_user AS usr;
```

```sql
SELECT schemaname, tablename
FROM pg_tables
WHERE schemaname = 'operational'
ORDER BY tablename;
```

Expected tables:
- job_lock
- job_master
- job_status
- job_queue
- job_queue_log

### 12.3 Foundation readiness checks for this plan

```sql
SELECT extname
FROM pg_extension
WHERE extname = 'pgcrypto';
```

```sql
SELECT COUNT(*) AS scheduled_job_master_rows
FROM fps.job_master
WHERE jobname = 'ScheduledLoadFromFps';
```

If scheduled_job_master_rows = 0, seed script 001 in Section 5 must insert it.

### 12.4 Cross-validation entry query (single report)
After assertion persistence is implemented, use:

```sql
SELECT
    run_id,
    COUNT(*) AS total_assertions,
    SUM(CASE WHEN passed THEN 1 ELSE 0 END) AS passed_assertions,
    SUM(CASE WHEN NOT passed THEN 1 ELSE 0 END) AS failed_assertions
FROM fps.scheduled_load_validation_result
GROUP BY run_id
ORDER BY run_id DESC;
```

Release criteria remains unchanged:
- failed_assertions must be 0 for all validation runs used as sign-off evidence.
