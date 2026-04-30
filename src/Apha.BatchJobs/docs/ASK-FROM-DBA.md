# Ask from DBA

## Purpose
This document captures the exact DBA asks needed to support BatchJobs foundation rollout and keep local and cloud aligned.

## Current Context
- Local foundation uses schema operational.
- Runtime relies on these tables:
  - fps.job_master
  - fps.job_status
  - fps.job_queue
  - fps.job_queue_log
  - fps.job_lock
- Cloud snapshot currently does not contain the operational tbljob* model.

## Priority 1 Ask (Blocking)
Please create and maintain the BatchJobs operational foundation schema objects in cloud:
- fps.job_master
- fps.job_status
- fps.job_queue
- fps.job_queue_log
- fps.job_lock

Use these source definitions as baseline:
- [src/Apha.BatchJobs/database/sql/001_batch_foundation_tables.sql](src/Apha.BatchJobs/database/sql/001_batch_foundation_tables.sql)
- [src/Apha.BatchJobs/database/sql/003_runtime_orchestrator_tables.sql](src/Apha.BatchJobs/database/sql/003_runtime_orchestrator_tables.sql)

## Priority 1 Ask (Lock Safety)
Please enforce DB-level single active lock per job:
- Add unique partial index on fps.job_lock(job_name) where is_active = true.

Expected index name:
- uq_job_lock_job_name_active

## Priority 2 Ask (Operational Hardening)
Please confirm and apply:
- Appropriate ownership and grants for application runtime role.
- Index maintenance plan for fps.job_lock and job_queue.
- Retention approach for old lock rows and queue logs.

## Validation Requested from DBA
Please provide confirmation output for:
- Table existence in schema operational for all 5 objects.
- Unique partial index existence on fps.job_lock.
- Basic insert/update path works for:
  - lock acquire/release flow on fps.job_lock
  - execution write flow on job_queue and job_queue_log

## Handover Back to Engineering
After DBA confirms, engineering will:
- Refresh cloud snapshot reference on VM.
- Re-run local vs cloud schema validation process.
- Verify lock contention behavior remains safe under concurrency.

## Contact Inputs DBA May Need
- Target environment details and DB endpoint (provided by platform team).
- Application role/service account used by BatchJobs runtime.
- Deployment window for non-breaking schema apply.

## New Ask: Exact Cloud-Structure Parity for ScheduledLoadFromFps

Engineering has aligned local migration `004_scheduled_load_tables.sql` to cloud-style table structures for business tables using:
- `money`, `double precision`, `smallint`
- legacy cloud naming conventions (`parentproject`, `totaladditionalcosts`, etc.)

Please confirm these are the exact canonical structures to follow:

1. `fps.fpsyeartotals` (source contract)
2. `mabarchive.my_fpsyeartotals` (year-keyed totals contract)
3. `mabarchive.my_tlkpproject_all` (project-all contract)

### Specific Clarifications Needed (Blocking for code mapping)

1. Should operational tables preserve legacy cloud column names exactly (`parentproject`) or use platform standard snake_case (`parent_project`)?
2. Should year use `smallint` (cloud archive) or `integer` (fps source has `fpsyear integer`)?
3. Is `money` mandatory for financial fields, or is `numeric(18,2)` acceptable for precision/portability?
4. For `fps_year_totals`: should `projectstatus` be nullable (as in `fps.fpsyeartotals`) or NOT NULL (as in `mabarchive.my_fpsyeartotals`)?
5. Confirm canonical primary key for totals in BatchJobs context:
  - Option A: `(year, parentproject)`
  - Option B: `parentproject` only (as in `fps.fpsyeartotals`)
6. For archive table design (`fps_year_archive`):
  - Should we keep typed columns only (cloud-like), or
  - include JSON payload for full immutable snapshots and easier replay?
7. Please provide exact index expectations for parity (cloud DDL extracts currently do not include all index details in snapshot CSV).

### Drift Noted (Cloud snapshot vs dbscript)

Current latest cloud snapshot includes tables not present in repo `dbscript` definitions:
- `fps.all_staff_project`
- `fps.all_wg_project`

Please confirm authoritative source for schema parity checks:
- live cloud snapshot exports (preferred), or
- repo `dbscript` folder.

## Update Applied Locally (For DBA Awareness)

Engineering has now created local boundary schemas for parity with cloud topology:
- `fps`
- `mabarchive`

These are currently empty by design in local foundation DB. Runtime execution remains in `operational`.

### Additional Confirmation Needed from DBA

1. Confirm whether `fps` and `mabarchive` should remain schema-only in local runtime DB, or if a minimum set of contract tables should also be provisioned locally.
2. Confirm required ownership/grants for these schemas in cloud and non-prod environments.
3. Confirm whether application `search_path` should include only `operational` (recommended), or include `fps/mabarchive` for read scenarios.

## New Ask: `qrytotaltestcosts` Must Include `fpsyear` (Blocking)

Please confirm and correct the canonical object definition for `fps.qrytotaltestcosts` so it includes:
- `jobcode`
- `fpsyear`
- `totaltestcosts`

Why this is blocking:
- Engineering must implement year-safe parity joins for ScheduledLoadFromFps.
- Without `fpsyear` in `qrytotaltestcosts`, test-cost joins can cross fiscal years and break strict parity.

Requested DBA action:
1. Verify live cloud definition for `fps.qrytotaltestcosts`.
2. If `fpsyear` is missing, update the object definition to include it.
3. Re-export schema metadata so `latest-cloud-schema-columns.csv` reflects the corrected contract.

Validation expected back from DBA:
- Object definition/DDL extract for `fps.qrytotaltestcosts`.
- Sample metadata row set showing columns: `jobcode`, `fpsyear`, `totaltestcosts`.

## New Ask: Post-Alignment DDL Parity Confirmation (Required)

Engineering has now aligned local required MABArchive tables to cloud **column metadata** parity using:
- table_schema
- table_name
- ordinal_position
- column_name
- data_type
- is_nullable
- column_default

Please provide canonical DDL confirmation for the same required table set covering metadata not present in the CSV export:

1. Primary keys, foreign keys, unique constraints, and check constraints.
2. Index definitions (including partial indexes) and expected index names.
3. Sequence ownership and increment/cache settings for identity/serial columns.
4. Table/column collation, storage parameters, and ownership/grants.
5. Any trigger/function dependencies required for parity in cloud.

Reason:
- The CSV reference is column-level only.
- We need full structural parity (constraints + indexes + sequence behavior) to guarantee runtime equivalence.

Requested DBA output:
- `pg_get_constraintdef` extract for required tables.
- `pg_indexes` extract for required tables.
- `information_schema.sequences` + ownership mapping for required schemas.
- Role/grant matrix for `fps` and `mabarchive`.

## New Ask: Schema Constraint Discovery for Test Data Loading (Blocking for MAB Archive Testing)

Engineering discovered during test scenario data load that `fps.tlkpproject` enforces strict NOT NULL constraints on multiple business columns that must be explicitly populated for INSERT operations.

### Discovered NOT NULL Constraints in fps.tlkpproject

The following columns enforce NOT NULL and must be provided for all project inserts:

| Column | Data Type | NOT NULL | Notes |
|---|---|---|---|
| parentproject | citext | YES | Project key/identifier |
| projecttitle | character varying(200) | YES | Display name |
| program | citext | YES | Program code reference |
| customer | citext | YES | **BLOCKING**: Required but not provided in initial test data |
| transferincome | money | YES | Required financial field |
| custincome | money | YES | Required financial field |
| projectstatus | citext | YES | Project status code |
| disease | citext | YES | Disease/program classification |
| contract | citext | YES | Contract indicator (e.g., 'Y' or 'N') |
| isdefraproject | smallint | YES | **BLOCKING**: Boolean flag, value required |
| incomeaccountcode | citext | YES | **BLOCKING**: Account code required |
| fpsyear | integer | YES | Fiscal year (e.g., 2026) |

### Impact on Test Data Loading

Engineering attempted to load test scenarios (P100-BASIC, P200-MULTI-A, P200-MULTI-B, P300-COMPLEX) into `fps.tlkpproject` but inserts failed due to:

1. Initial missing columns: `customer`, `contract`, `isdefraproject`, `incomeaccountcode`
2. Local schema enforces stricter NOT NULL than expected from documentation

### Schema Alignment Request

Please confirm from DBA:

1. **Canonical column list**: Provide complete list of NOT NULL columns in production `fps.tlkpproject` to ensure local test data schema matches exactly.
2. **Default values**: For columns like `isdefraproject` and `incomeaccountcode`, what are typical default/seed values for test scenarios?
3. **Column metadata export**: Update `latest-cloud-schema-columns.csv` to include:
   - is_nullable
   - column_default
   - For all columns in fps.tlkpproject and related project tables

### Engineering Action (Pending DBA Confirmation)

Once defaults/constraints are confirmed, engineering will:
1. Update `200_insert_test_scenario_data.sql` to include all required columns with appropriate test values.
2. Re-run test scenario loads and validate Phase 1 source data validation passes.
3. Proceed to Phase 4 (Run MAB Archive job) and Phase 5 (Archive validation).

### Current Local State

- `fps.tlkpproject` schema exists but test projects cannot be inserted until above columns are provided.
- `fps.fpsyeartotals` baseline project (P100-BASELINE) was successfully loaded with explicit column population.
- `fps.timecostcalcs`, `fps.monthlyoutput`, `fps.monthlytime` tables exist but cannot populate test data until parent projects load.

## Engineering Change Log: DDL and DB Runtime Changes (2026-04-30)

### DDL Executed by Engineering

No permanent DDL was executed in local PostgreSQL during the Jan-Apr / May-Dec branch testing runs.

- No `CREATE TABLE`
- No `ALTER TABLE`
- No `DROP TABLE`
- No `CREATE VIEW` / `ALTER VIEW`

### Runtime DB Data/Sequence Maintenance Executed

To unblock test execution only (non-DDL):

1. Cleared stale active lock rows in `fps.job_lock` by setting `is_active=false` for expired/blocked MABArchive/ScheduleJobs lock entries.
2. Re-synced identity sequences to max row ids to avoid duplicate key failures:
  - `fps.job_status_statusid_seq`
  - `fps.job_queue_log_jobqueuelogid_seq`

These were operational data maintenance actions and are not schema changes.

### Cloud/DBA DDL Follow-up Needed

During May-branch full-cycle testing, additional schema-contract drift was observed:

1. `fps.qrytotaladditionalcosts`, `fps.qrytotalanimalcosts`, `fps.qrytotalstaffcosts`, `fps.qrytotaltestcosts` are currently keyed by `jobcode` only in local Postgres (no `fpsyear` column).
2. MABArchive totals rebuild logic requires canonical agreement on whether these views should be year-keyed to prevent cross-year joins.
3. Financial type behavior uses PostgreSQL `money`; casts and arithmetic semantics should be confirmed as canonical for cloud parity.

Requested DBA response:

- Confirm canonical DDL for all four `qrytotal*costs` views including whether `fpsyear` must be present.
- Confirm canonical datatype strategy for cost arithmetic (`money` vs `numeric(18,2)`), including casting guidance for aggregate/rebuild logic.

## Engineering Change Log Delta (2026-04-30 Later Update)

### DDL Executed by Engineering

No additional permanent DDL was executed after the previous change-log entry.

- No `CREATE TABLE`
- No `ALTER TABLE`
- No `DROP TABLE`
- No `CREATE VIEW` / `ALTER VIEW`

### Runtime/Test Outcomes (Jan-Apr and May-Dec)

1. Jan-Apr forced branch test (`MABARCHIVE_TEST_UTCNOW=2026-03-15`) completed successfully.
2. May-Dec forced branch test (`MABARCHIVE_TEST_UTCNOW=2026-05-15`) now also completed successfully end-to-end after engineering runtime/code fixes.

### New Schema-Contract Observation for DBA Confirmation

During May full-cycle loader validation, `mabarchive.my_tbladditionalcosts.ac_counter` was observed as:
- `NOT NULL`
- primary key
- no default in local schema contract

Engineering applied an application-side fallback (no DDL) to populate `ac_counter` during loader insert for deterministic non-null values.

Please confirm canonical cloud contract for `mabarchive.my_tbladditionalcosts.ac_counter`:
1. Should this column be backed by a sequence/default in canonical DDL?
2. If yes, provide the expected default expression and sequence ownership settings.
3. If no, confirm loader/application is expected to supply `ac_counter` explicitly.
