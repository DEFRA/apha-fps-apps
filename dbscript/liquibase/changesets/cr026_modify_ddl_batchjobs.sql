--liquibase formatted sql

--changeset repo-admin:CR026 labels:ddl context:all

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS heartbeat_at_utc TIMESTAMP NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS failure_reason TEXT NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS ended_at_utc TIMESTAMP NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS updated_at_utc TIMESTAMP NULL;

ALTER TABLE fps.job_lock
ADD COLUMN IF NOT EXISTS lock_expires_at_utc TIMESTAMP NULL;

UPDATE fps.job_lock
SET lock_expires_at_utc = COALESCE(lock_expires_at_utc, expires_at, NOW() + INTERVAL '20 minutes')
WHERE lock_expires_at_utc IS NULL;

ALTER TABLE fps.job_lock
ALTER COLUMN lock_expires_at_utc SET NOT NULL;

CREATE INDEX IF NOT EXISTS idx_job_queue_statusid_heartbeat_at_utc
ON fps.job_queue(statusid, heartbeat_at_utc);

CREATE INDEX IF NOT EXISTS idx_job_lock_lock_expires_at_utc
ON fps.job_lock(lock_expires_at_utc);

WITH stale_jobs AS (
    SELECT jq.jobqueueid,
           jq.jobid
    FROM fps.job_queue jq
    INNER JOIN fps.job_status js
        ON js.statusid = jq.statusid
    WHERE js.status IN ('Running', 'Initiated')
      AND COALESCE(jq.heartbeat_at_utc, jq.updated_at_utc, jq.startdatetime)
            < NOW() - INTERVAL '15 minutes'
)
UPDATE fps.job_queue q
SET statusid = failed_status.statusid,
    failure_reason = COALESCE(
        q.failure_reason,
        'Stale in-progress record recovered by can-run reconciliation'
    ),
    ended_at_utc = COALESCE(q.ended_at_utc, NOW()),
    updated_at_utc = NOW()
FROM stale_jobs s
INNER JOIN fps.job_status failed_status
    ON failed_status.jobid = s.jobid
   AND failed_status.status = 'Failed'
WHERE q.jobqueueid = s.jobqueueid;

DELETE FROM fps.job_lock
WHERE lock_expires_at_utc < NOW();

--ROLLBACK
--Not Applicable