-- Migration: persist trigger acceptance timestamp on execution rows.
--
-- Adds nullable requested_at_utc to fps.job_queue so worker can store
-- BATCH_REQUESTED_AT_UTC using existing snake_case timestamp naming.

BEGIN;

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS requested_at_utc TIMESTAMPTZ;

CREATE INDEX IF NOT EXISTS idx_job_queue_requested_at_utc
    ON fps.job_queue (requested_at_utc);

COMMIT;

-- Verification:
-- SELECT column_name, data_type, is_nullable
-- FROM information_schema.columns
-- WHERE table_schema = 'fps' AND table_name = 'job_queue' AND column_name = 'requested_at_utc';
