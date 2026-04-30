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

## 11) Local DB final status (batch_jobs_foundation_db)

Final verification completed on 2026-04-30 in local database: batch_jobs_foundation_db.

### Schemas present
- fps
- mabarchive
- operational

### Runtime migration status
- operational legacy runtime tables were migrated to fps runtime tables:
  - operational.batch_lock -> fps.job_lock
  - operational.tbljobmaster -> fps.job_master
  - operational.tbljobstatus -> fps.job_status
  - operational.tbljobqueue -> fps.job_queue
  - operational.tbljobqueue_log -> fps.job_queue_log
- backward-compatibility views exist in operational:
  - operational.batch_lock
  - operational.tbljobmaster
  - operational.tbljobstatus
  - operational.tbljobqueue
  - operational.tbljobqueue_log

### FPS source footprint status for MAB Archive
- all required fps source tables are present (including dependency chain tables created from dbscript/schemas/01fps/01tables)
- all required totals views are present:
  - fps.qrytotaladditionalcosts
  - fps.qrytotalanimalcosts
  - fps.qrytotalstaffcosts
  - fps.qrytotaltestcosts

### Notes on strict source usage
- dependency tables were created using canonical SQL files from dbscript/schemas/01fps/01tables.
- tblyearmaster does not have a canonical table SQL file under dbscript/schemas/01fps/01tables in this repository, so it was created to match EF mapping expectations (fpsyear PK, unique fpsyearcode, status/active/audit columns).

## 12) Consolidated verification checklist result

A single checklist query was executed across required schemas, tables, and views.

- Total checked: 41 objects
- Present: 41
- Missing: 0

This includes:
- 3 required schemas
- 29 required tables (runtime + MAB Archive source)
- 9 required views (fps totals views + operational compatibility views)

## 13) Operational conclusion

The local DB is now aligned with the long-term target model for this workflow:

- runtime orchestration uses fps.job_* tables
- MAB Archive source objects in fps are available
- MAB Archive target tables in mabarchive are available
- compatibility layer remains available under operational views

The environment is ready for end-to-end ScheduledLoadFromFps and MAB Archive execution.

## 14) Migration assets used

Migration and validation SQL scripts:
- database/sql/100_migrate_operational_to_fps_runtime_tables.sql
- database/sql/101_rollback_operational_to_fps_migration.sql
- database/sql/102_validate_operational_to_fps_migration.sql

Supporting local helper script used for dependency/object completion:
- src/Apha.BatchJobs/database/sql/apply_needed_fps_dependencies.ps1
