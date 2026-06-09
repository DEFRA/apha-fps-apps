# Local Batch Database Implementation

This document describes the latest local batch database implementation for `Apha.BatchJobs` in the `fps` schema.

The authoritative local mappings are in:

- [src/Apha.BatchJobs/docs/database/sql/001_batch_foundation_tables.sql](src/Apha.BatchJobs/docs/database/sql/001_batch_foundation_tables.sql)
- [src/Apha.BatchJobs/docs/database/sql/003_runtime_orchestrator_tables.sql](src/Apha.BatchJobs/docs/database/sql/003_runtime_orchestrator_tables.sql)
- [src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs)
- [src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/OperationalScheduledLoadTables.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/OperationalScheduledLoadTables.cs)
- [src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Unit/EfCoreMappingTests.cs](src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Unit/EfCoreMappingTests.cs)

## Current Scope

The current local implementation centers on two layers:

1. Batch orchestration tables for execution control and auditing.
2. Scheduled-load tables for run lifecycle, step tracking, and validation results.

The local implementation also includes a dedicated lock table for single-flight execution control.

## Orchestration Tables

### `fps.job_master`

Purpose: stores the canonical list of batch jobs and their runtime policy.

Typical use:

- Defines which jobs are available to the worker/orchestrator.
- Carries the job name, schedule cadence, and allowed runtime window.
- Serves as the parent table for per-job status rows.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `jobid` | integer identity | Primary key |
| `jobname` | varchar(100) | Unique job name |
| `frequency` | varchar(50) | Optional schedule label |
| `note` | varchar(250) | Optional description |
| `timetolive` | integer | Required, positive runtime limit |
| `created_at` | timestamptz | Default `NOW()` |
| `updated_at` | timestamptz | Default `NOW()` |

Key relationships:

- Referenced by `fps.job_status`
- Referenced by `fps.job_queue`
- Referenced by `fps.scheduled_load_run` through `JobName`

### `fps.job_status`

Purpose: stores the allowed status values for each job definition.

Typical use:

- Models per-job lifecycle states such as pending, running, completed, failed, or cancelled.
- Enforces that the valid status set belongs to a specific job.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `statusid` | integer identity | Primary key |
| `jobid` | integer | Foreign key to `fps.job_master(jobid)` |
| `status` | varchar(100) | Status label |
| `created_at` | timestamptz | Default `NOW()` |

Key relationships:

- `jobid` is a cascading foreign key to `fps.job_master`
- Used by `fps.job_queue` and `fps.job_queue_log`

### `fps.job_queue`

Purpose: stores one row per execution instance of a batch job.

Typical use:

- Tracks execution start and end times.
- Holds the current status for the execution.
- Records who requested the job and any error message on failure.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `jobqueueid` | uuid | Primary key, default `gen_random_uuid()` |
| `jobexecutionid` | uuid | Required, unique external execution id |
| `jobid` | integer | Foreign key to `fps.job_master(jobid)` |
| `statusid` | integer | Foreign key to `fps.job_status(statusid)` |
| `requestedby` | varchar(100) | Required requester identity |
| `startdatetime` | timestamptz | Required start timestamp |
| `enddatetime` | timestamptz | Optional completion timestamp |
| `errormessage` | varchar(1000) | Optional failure detail |
| `created_at` | timestamptz | Default `NOW()` |
| `updated_at` | timestamptz | Default `NOW()` |

Key relationships and constraints:

- `jobid` deletes are restricted.
- `statusid` deletes are restricted.
- `jobexecutionid` is uniquely indexed.
- `requestedby` is indexed for lookup.
- `enddatetime` must be greater than or equal to `startdatetime` when present.

### `fps.job_queue_log`

Purpose: stores the chronological audit trail for each queued execution.

Typical use:

- Records state changes during execution.
- Captures operator or system notes.
- Provides event-by-event traceability for a run.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `jobqueuelogid` | integer identity | Primary key |
| `jobqueueid` | uuid | Foreign key to `fps.job_queue(jobqueueid)` |
| `statusid` | integer | Foreign key to `fps.job_status(statusid)` |
| `performedby` | varchar(100) | Actor who wrote the log entry |
| `logtime` | timestamptz | Default `NOW()` |
| `note` | varchar(500) | Optional note |

Key relationships:

- `jobqueueid` deletes cascade from `fps.job_queue`
- `statusid` remains restricted through `fps.job_status`
- Indexed by execution and timestamp for chronological reads

### `fps.job_lock`

Purpose: prevents duplicate concurrent execution of the same batch job.

Typical use:

- Enforces single-flight execution for a job name.
- Lets the orchestrator detect and reuse or release active locks.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `lock_id` | integer identity | Primary key |
| `job_name` | varchar(255) | Locked job identifier |
| `acquired_at` | timestamptz | When the lock was acquired |
| `expires_at` | timestamptz | Lock expiry time |
| `jobqueueid` | uuid | Associated execution id |
| `is_active` | boolean | Default `true` |

Key relationships and constraints:

- Partial unique index ensures only one active lock per `job_name`.
- Indexed by `job_name` and `expires_at` for lock lookup and expiration checks.

## Scheduled Load Tables

The scheduled-load tables are the latest local run ledger for the batch workflow. They are EF-mapped in [BatchJobsDbContext.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs) and modeled in [OperationalScheduledLoadTables.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Data/OperationalScheduledLoadTables.cs).

### `fps.scheduled_load_run`

Purpose: stores one row per scheduled batch run.

Typical use:

- Represents the overall run lifecycle.
- Groups all step runs and validation results under one run id.
- Carries the business year and trace correlation id.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `run_id` | uuid | Primary key, default `gen_random_uuid()` |
| `job_name` | varchar(100) | Required job name |
| `fps_year` | integer | Business year for the run |
| `job_started_at` | timestamptz | Required start time |
| `job_completed_at` | timestamptz | Optional completion time |
| `final_status` | varchar(50) | Optional terminal state |
| `correlation_id` | varchar(64) | Required trace id |
| `created_at` | timestamptz | Default `NOW()` |

Key relationships and constraints:

- `job_name` is a foreign key to `fps.job_master(jobname)`.
- Indexed by `(job_name, fps_year)`.
- Indexed by `correlation_id`.

### `fps.scheduled_load_step_run`

Purpose: stores each step within a scheduled run.

Typical use:

- Tracks ordered workflow steps.
- Captures timing, row counts, and error messages per step.
- Supports diagnosing where a run failed or stalled.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `step_run_id` | uuid | Primary key, default `gen_random_uuid()` |
| `run_id` | uuid | Foreign key to `fps.scheduled_load_run(run_id)` |
| `step_name` | varchar(100) | Required step label |
| `step_sequence` | integer | Required step order |
| `started_at` | timestamptz | Required start time |
| `completed_at` | timestamptz | Optional completion time |
| `step_status` | varchar(50) | Required status |
| `error_message` | varchar(500) | Optional failure detail |
| `rows_affected` | integer | Optional row count |
| `created_at` | timestamptz | Default `NOW()` |

Key relationships and constraints:

- Cascades on delete from the parent run.
- Indexed by `run_id` and `step_status`.

### `fps.scheduled_load_validation_result`

Purpose: stores validation and assertion results for a run.

Typical use:

- Records pass/fail checks after or during a batch execution.
- Captures expected versus actual values for diagnostics.
- Provides a durable audit trail for data quality assertions.

Columns:

| Column | Type | Notes |
| --- | --- | --- |
| `validation_id` | uuid | Primary key, default `gen_random_uuid()` |
| `run_id` | uuid | Foreign key to `fps.scheduled_load_run(run_id)` |
| `assertion_code` | varchar(50) | Required assertion identifier |
| `assertion_description` | varchar(500) | Required human-readable description |
| `expected_value` | numeric(18,2) | Optional expected value |
| `actual_value` | numeric(18,2) | Optional observed value |
| `passed` | boolean | Required outcome flag |
| `error_message` | varchar(500) | Optional failure detail |
| `checked_at` | timestamptz | Required check time |
| `created_at` | timestamptz | Default `NOW()` |

Key relationships and constraints:

- Cascades on delete from the parent run.
- Unique constraint on `(run_id, assertion_code)`.
- Indexed by `(run_id, passed)` and `assertion_code`.

## Local Implementation Notes

- The orchestration tables are the active runtime schema for worker execution and locking.
- The scheduled-load tables are the current local implementation for run lifecycle tracking and validation.
- The EF model is the latest source of truth for the local implementation details, including lengths, defaults, indexes, and delete behavior.
- Older intermediate tables such as `fps.fps_source_project_year`, `fps.fps_year_totals`, `fps.fps_year_archive`, and `fps.fps_project_all_current_year` are intentionally retired by the current migration set.
- The deprecated `operational` schema is dropped in the current local database direction.

## Quick Reference

| Table | Purpose |
| --- | --- |
| `fps.job_master` | Job definitions and runtime policy |
| `fps.job_status` | Allowed statuses per job |
| `fps.job_queue` | One row per execution instance |
| `fps.job_queue_log` | Execution audit trail |
| `fps.job_lock` | Active lock tracking |
| `fps.scheduled_load_run` | Batch run lifecycle |
| `fps.scheduled_load_step_run` | Per-step execution tracking |
| `fps.scheduled_load_validation_result` | Validation/assertion results |
