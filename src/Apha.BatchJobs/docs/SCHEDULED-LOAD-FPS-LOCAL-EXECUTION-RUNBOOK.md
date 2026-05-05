# Local Execution Runbook: ScheduledLoadFromFps

## Prerequisites
- Docker running with Postgres container `batch_jobs_postgres`.
- Access to local DB `batch_jobs_foundation_db`.
- Shell access with `psql` available.
- .NET SDK installed where `dotnet run` is executed.

## Quick Start

### 1) Ensure database is reachable
```bash
docker ps | grep batch_jobs_postgres
```

### 2) Reset to known state
```bash
bash database/sql/reset_scheduled_load_locally.sh
```

### 3) Execute ScheduledLoadFromFps
```bash
cd /workspaces/apha-fps-apps/src/Apha.BatchJobs
dotnet run --project BatchJobs.csproj -- --mode cli --job ScheduledLoadFromFps
```

## Verification Queries

```sql
SELECT run_id, job_name, fps_year, final_status, created_at
FROM fps.scheduled_load_run
ORDER BY created_at DESC
LIMIT 1;
```

```sql
SELECT step_name, step_status, rows_affected, started_at, completed_at
FROM fps.scheduled_load_step_run
ORDER BY started_at DESC
LIMIT 10;
```

```sql
SELECT assertion_code, passed, expected_value, actual_value, checked_at
FROM fps.scheduled_load_validation_result
ORDER BY checked_at DESC
LIMIT 20;
```

```sql
SELECT year, COUNT(*) AS projects, SUM(totalcosts) AS total_costs
FROM mabarchive.my_fpsyeartotals
GROUP BY year
ORDER BY year;
```

## Common Scenarios

### Pre-cutover execution path
Run with a forced month before cutover (via app settings override) and verify `ProcessCurrentYearTotals` is absent from step audit.

### Post-cutover execution path
Run with current month after cutover and verify all 5 steps execute.

### Year-slice archive refresh
Seed a stale row in `mabarchive.my_fpsyeartotals` for target year, run the job, and verify stale row is removed.

## Troubleshooting

### Job not found
Symptom: `Job not found` during startup.
Action:
```sql
SELECT jobid, jobname FROM fps.job_master WHERE jobname = 'ScheduledLoadFromFps';
```
If missing, run seed 001.

### FK violation on scheduled_load_run
Symptom: `fk_scheduled_load_run_jobname` violation.
Action: ensure seed 001 ran successfully and inserted `ScheduledLoadFromFps` in `fps.job_master`.

### No step audit rows
Symptom: run exists but `scheduled_load_step_run` is empty.
Action: check app logs for early exceptions and verify step handlers are registered in DI.

### Validation failures
Symptom: rows in `scheduled_load_validation_result` with `passed = false`.
Action:
```sql
SELECT assertion_code, assertion_description, error_message
FROM fps.scheduled_load_validation_result
WHERE passed = FALSE
ORDER BY checked_at DESC;
```

## Reset Guidance
- Preferred: `bash database/sql/reset_scheduled_load_locally.sh`
- Focused data wipe only: `database/sql/flush/002_flush_scheduled_load_tables.sql`
- Full foundation wipe: `database/sql/flush/001_flush_operational_schema.sql`
