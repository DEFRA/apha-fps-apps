--liquibase formatted sql

--changeset repo-admin:CR016 labels:ddl context:all

-- 1) job_queue lifecycle/heartbeat columns
ALTER TABLE fps.job_queue
  ADD COLUMN IF NOT EXISTS heartbeat_at_utc TIMESTAMP NULL,
  ADD COLUMN IF NOT EXISTS failure_reason  TEXT NULL,
  ADD COLUMN IF NOT EXISTS ended_at_utc    TIMESTAMP NULL,
  ADD COLUMN IF NOT EXISTS updated_at_utc  TIMESTAMP NULL;

-- 2) lock expiry column
ALTER TABLE fps.job_lock
  ADD COLUMN IF NOT EXISTS lock_expires_at_utc TIMESTAMP NOT NULL
  DEFAULT (NOW() + INTERVAL '20 minutes');

-- 3) indexes for reconciliation queries
CREATE INDEX IF NOT EXISTS idx_job_queue_statusid_heartbeat_at_utc
  ON fps.job_queue(statusid, heartbeat_at_utc);
 -- WHERE status = 'Running'; commented since not recomending filtered indexes.

CREATE INDEX IF NOT EXISTS idx_job_lock_lock_expires_at_utc
  ON fps.job_lock(lock_expires_at_utc);
 -- WHERE lock_expires_at_utc IS NOT NULL; commented since not recomending filtered indexes.

--ROLLBACK
--Not Applicable

