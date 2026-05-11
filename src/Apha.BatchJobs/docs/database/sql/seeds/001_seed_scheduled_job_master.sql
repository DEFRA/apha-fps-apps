-- Seed: register ScheduledLoadFromFps in job_master and default statuses in job_status.
-- Safe to re-run: uses ON CONFLICT guards.

BEGIN;

WITH upsert_job AS (
    INSERT INTO fps.job_master (
        jobname,
        frequency,
        note,
        timetolive,
        created_at,
        updated_at
    )
    VALUES (
        'ScheduledLoadFromFps',
        'Monthly',
        'Scheduled batch job for loading and transforming FPS data from cloud snapshot',
        3600,
        NOW(),
        NOW()
    )
    ON CONFLICT (jobname)
    DO UPDATE SET
        frequency = EXCLUDED.frequency,
        note = EXCLUDED.note,
        timetolive = EXCLUDED.timetolive,
        updated_at = NOW()
    RETURNING jobid
),
job_ref AS (
    SELECT jobid FROM upsert_job
    UNION ALL
    SELECT jm.jobid
    FROM fps.job_master jm
    WHERE jm.jobname = 'ScheduledLoadFromFps'
    LIMIT 1
)
INSERT INTO fps.job_status (jobid, status, created_at)
SELECT jr.jobid, s.status, NOW()
FROM job_ref jr
CROSS JOIN (
    VALUES
        ('Queued'),
        ('Running'),
        ('Completed'),
        ('Failed'),
        ('Cancelled')
) AS s(status)
ON CONFLICT (jobid, status) DO NOTHING;

COMMIT;
