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
