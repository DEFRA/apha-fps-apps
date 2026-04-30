# MAB Archive Process - Required SQL Objects

## Purpose
This document lists the SQL objects required to run the MAB Archive process end to end.

It covers:
- runtime control objects (lock and execution tracking),
- source objects used to calculate and pick data, and
- target archive objects where data is written.

## 1) Required schemas
- fps
- mabarchive

## 2) Runtime control objects (job execution + locking)

These are needed by the batch runtime around the MAB Archive job.

### Required tables
- fps.job_lock
- fps.job_master
- fps.job_status
- fps.job_queue
- fps.job_queue_log

### Required constraints and indexes (critical)
- unique active-lock rule on fps.job_lock so only one active run can hold lock for a job name
- index on fps.job_lock(job_name)
- index on fps.job_lock(job_name, is_active)
- index on fps.job_lock(expires_at)
- unique key on fps.job_master(jobname)
- unique key on fps.job_status(jobid, status)
- foreign keys:
  - fps.job_status.jobid -> fps.job_master.jobid
  - fps.job_queue.jobid -> fps.job_master.jobid
  - fps.job_queue.statusid -> fps.job_status.statusid
  - fps.job_queue_log.jobqueueid -> fps.job_queue.jobqueueid
  - fps.job_queue_log.statusid -> fps.job_status.statusid

## 3) Year availability check object

Used before yearly processing starts.

### Required table
- fps.tblyearmaster

### Required column usage
- fps.tblyearmaster.fpsyear

## 4) Source totals rebuild objects (inside fps)

These are used to rebuild fps.fpsyeartotals each run.

### Required source table
- fps.tlkpproject

### Required source views/tables for joined totals
- fps.qrytotaladditionalcosts
- fps.qrytotalanimalcosts
- fps.qrytotalstaffcosts
- fps.qrytotaltestcosts

### Required target table (still in fps schema)
- fps.fpsyeartotals

## 5) Source objects for yearly archive load

Used to copy data into mabarchive tables.

### Required fps source tables
- fps.tlkpprogram
- fps.tlkpproject
- fps.fpsyeartotals
- fps.monthlyoutput
- fps.monthlytime
- fps.proj_invoice
- fps.proj_subcontract
- fps.projectmonthfinal
- fps.tbladditionalcosts
- fps.tblanimalreq
- fps.tblcontract
- fps.tblstaffjob
- fps.timecostcalcs
- fps.tlkptestreqmt
- fps.tbldb_variables
- fps.workgroupgrade
- fps.profitcentregrade
- fps.tblkpprofitcentre
- fps.testorproduct
- fps.tblwgemployee
- fps.tblemployee
- fps.workgroup
- fps.tblanimals

Notes:
- fps.tbldb_variables is used for month metadata (db_var_name = 'month').
- fps.tblkpprofitcentre is copied as shared reference data (no fpsyear predicate in query).

## 6) Target objects in mabarchive (delete + reload)

These tables are deleted and reloaded for the selected year (except project-based special case).

### Required mabarchive target tables
- mabarchive.my_tlkpprogram
- mabarchive.g_tlkpproject
- mabarchive.my_tlkpproject
- mabarchive.my_fpsyeartotals
- mabarchive.my_monthlyoutput
- mabarchive.my_monthlytime
- mabarchive.my_proj_invoice
- mabarchive.my_proj_subcontract
- mabarchive.my_projectmonthfinal
- mabarchive.my_tbladditionalcosts
- mabarchive.my_tblanimalreq
- mabarchive.my_tblcontract
- mabarchive.my_tblstaffjob
- mabarchive.my_timecostcalcs
- mabarchive.my_tlkptestreqmt
- mabarchive.tlkpyear
- mabarchive.my_workgroupgrade
- mabarchive.my_profitcentregrade
- mabarchive.my_tblprofitcentre
- mabarchive.my_testorproduct
- mabarchive.my_staff
- mabarchive.my_workgroup
- mabarchive.my_tblanimals
- mabarchive.my_tlkpproject_all

## 7) Partial-refresh object (Jan-Apr branch)

When month <= 4, current year partial refresh touches only:
- mabarchive.my_tlkpproject_all (delete current-year slice, then reload from fps.tlkpproject)

## 8) Object count summary
- Schemas: 2
- Runtime control tables: 5
- Year check table: 1
- Totals rebuild objects: 6 (1 target + 5 source/join objects)
- Yearly load source tables: 23
- Archive target tables: 24

## 9) Clarification on stored procedures
No SQL stored procedures are required by the current .NET MAB Archive implementation.
The process executes SQL statements directly from application code.

## 10) Implementation references
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/BatchLockRepository.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/JobExecutionRepository.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs

## 11) Local DB assessment (batch_jobs_foundation_db)

Checked on 2026-04-30 in local database: batch_jobs_foundation_db.

Current runtime objects found:
- schema operational exists
- tables in operational:
  - operational.batch_lock
  - operational.tbljobmaster
  - operational.tbljobstatus
  - operational.tbljobqueue
  - operational.tbljobqueue_log

Not found in this DB at check time:
- schema fps
- runtime tables named fps.job_lock, fps.job_master, fps.job_status, fps.job_queue, fps.job_queue_log

## 12) Can we move operational runtime tables to fps schema?

Short answer: yes, this can be done, and it is recommended for consistency with current code expectations.

Important clarity:
- This is not only a schema move.
- Your local objects use legacy names (batch_lock, tbljob*).
- Current runtime code expects modern names (job_lock, job_*).

Required target naming for alignment:
- operational.batch_lock -> fps.job_lock
- operational.tbljobmaster -> fps.job_master
- operational.tbljobstatus -> fps.job_status
- operational.tbljobqueue -> fps.job_queue
- operational.tbljobqueue_log -> fps.job_queue_log

Also required after move/rename:
- recreate/validate all expected foreign keys and indexes
- especially the active lock uniqueness behavior on fps.job_lock
- verify application can insert/update queue and queue_log rows end to end

## 13) Recommended migration approach

Use a controlled migration script in this order:
1. Create schema fps if missing.
2. Create/rename target tables to expected names in fps schema.
3. Copy data from operational legacy tables to fps tables.
4. Recreate constraints and indexes with expected names.
5. Validate record counts and FK integrity.
6. Run one test job execution and verify lock + queue + queue_log writes.
7. Keep operational tables as backup for one release window, then retire.

If you want, the next step can be a concrete SQL migration script for this exact local state.

## Migration Status: ✅ COMPLETED

**Date:** 2026-04-30  
**Database:** batch_jobs_foundation_db

The migration from operational legacy schema to fps modern schema has been successfully completed:

- ✅ fps schema created
- ✅ 5 runtime tables created with normalized names (job_lock, job_master, job_status, job_queue, job_queue_log)
- ✅ All 47 data rows migrated with 100% integrity
- ✅ All constraints, indexes, and foreign keys recreated
- ✅ Backward-compatibility views established in operational schema
- ✅ All referential integrity checks pass (0 orphaned records)
- ✅ All check constraints valid (0 violations)

**Migration Scripts:**
- `database/sql/100_migrate_operational_to_fps_runtime_tables.sql` — Primary migration with 14 sequential steps
- `database/sql/101_rollback_operational_to_fps_migration.sql` — Safe rollback procedure
- `database/sql/102_validate_operational_to_fps_migration.sql` — Comprehensive validation script

**Documentation:**
- `docs/MIGRATION_OPERATIONAL_TO_FPS_COMPLETION.md` — Full migration report with data counts and object inventory

### Result
The runtime orchestration layer is now properly aligned with EF Core's expected schema structure. All job execution, locking, and queue operations can now use fps schema directly. Legacy operational tables have been preserved as `*_legacy` for a transition period.
