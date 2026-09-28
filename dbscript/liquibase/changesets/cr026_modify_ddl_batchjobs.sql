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

-- Stale job/lock reconciliation DML moved to postdbscripts/cr026_cleanup_stale_jobs_and_locks.sql

--ROLLBACK
--Not Applicable