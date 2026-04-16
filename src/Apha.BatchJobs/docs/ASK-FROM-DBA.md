# Ask from DBA

## Purpose
This document captures the exact DBA asks needed to support BatchJobs foundation rollout and keep local and cloud aligned.

## Current Context
- Local foundation uses schema operational.
- Runtime relies on these tables:
  - operational.tbljobmaster
  - operational.tbljobstatus
  - operational.tbljobqueue
  - operational.tbljobqueue_log
  - operational.batch_lock
- Cloud snapshot currently does not contain the operational tbljob* model.

## Priority 1 Ask (Blocking)
Please create and maintain the BatchJobs operational foundation schema objects in cloud:
- operational.tbljobmaster
- operational.tbljobstatus
- operational.tbljobqueue
- operational.tbljobqueue_log
- operational.batch_lock

Use these source definitions as baseline:
- [src/Apha.BatchJobs/database/sql/001_batch_foundation_tables.sql](src/Apha.BatchJobs/database/sql/001_batch_foundation_tables.sql)
- [src/Apha.BatchJobs/database/sql/003_runtime_orchestrator_tables.sql](src/Apha.BatchJobs/database/sql/003_runtime_orchestrator_tables.sql)

## Priority 1 Ask (Lock Safety)
Please enforce DB-level single active lock per job:
- Add unique partial index on operational.batch_lock(job_name) where is_active = true.

Expected index name:
- uq_batch_lock_job_name_active

## Priority 2 Ask (Operational Hardening)
Please confirm and apply:
- Appropriate ownership and grants for application runtime role.
- Index maintenance plan for operational.batch_lock and tbljobqueue.
- Retention approach for old lock rows and queue logs.

## Validation Requested from DBA
Please provide confirmation output for:
- Table existence in schema operational for all 5 objects.
- Unique partial index existence on operational.batch_lock.
- Basic insert/update path works for:
  - lock acquire/release flow on operational.batch_lock
  - execution write flow on tbljobqueue and tbljobqueue_log

## Handover Back to Engineering
After DBA confirms, engineering will:
- Refresh cloud snapshot reference on VM.
- Re-run local vs cloud schema validation process.
- Verify lock contention behavior remains safe under concurrency.

## Contact Inputs DBA May Need
- Target environment details and DB endpoint (provided by platform team).
- Application role/service account used by BatchJobs runtime.
- Deployment window for non-breaking schema apply.
