-- 106_add_missing_job_statuses.sql
-- Purpose:
--   Add missing job status values (Pending, Retry, Skipped) to fps.job_status
--   to align with C# JobStatus enum and support extended workflow states.
--
-- Changes:
--   1. Insert Pending, Retry, Skipped statuses for all existing jobs
--   2. Ensure Running, Completed, Failed, Cancelled exist for all jobs
--
-- Execution:
--   Safe to re-run: uses INSERT ... ON CONFLICT DO NOTHING to handle duplicates.

BEGIN;

-- Ensure all required statuses exist for each job
-- This creates the Cartesian product of all jobs × all statuses
INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, status_value
FROM fps.job_master jm
CROSS JOIN (
    VALUES 
        ('Pending'),
        ('Running'),
        ('Completed'),
        ('Failed'),
        ('Cancelled'),
        ('Retry'),
        ('Skipped')
) AS statuses(status_value)
ON CONFLICT (jobid, status) DO NOTHING;

COMMIT;

-- Optional: Verify the insertion
-- SELECT jm.jobname, js.status, js.statusid, js.created_at
-- FROM fps.job_master jm
-- LEFT JOIN fps.job_status js ON jm.jobid = js.jobid
-- ORDER BY jm.jobname, js.status;
