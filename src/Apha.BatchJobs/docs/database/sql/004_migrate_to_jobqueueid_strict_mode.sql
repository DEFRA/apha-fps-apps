-- Migration: Canonical execution identity model for polling and auditability
-- 
-- Changes:
-- 1. Update job_lock table schema: replace run_id (varchar) with jobqueueid (UUID)
-- 2. Add external execution identity and requester metadata on job_queue
-- 3. Ensure worker-owned jobqueueid has DEFAULT gen_random_uuid()
--
-- Backward compatibility: 
-- - Existing locks in job_lock will be cleared during deployment (assumed not critical for migrations)
-- - Existing queue rows are backfilled with generated jobexecutionid and requestedby='system'

BEGIN;

-- Step 1: Clear any existing locks (old schema won't match new schema anyway)
DELETE FROM fps.job_lock;

-- Step 2: Drop old run_id column and indexes that reference it
ALTER TABLE fps.job_lock
    DROP COLUMN IF EXISTS run_id;

-- Step 3: Add jobqueueid column if not already present
ALTER TABLE fps.job_lock
    ADD COLUMN IF NOT EXISTS jobqueueid UUID NOT NULL UNIQUE;

-- Step 4: Recreate partial unique index for strict mode (one active lock per job)
DROP INDEX IF EXISTS fps.uq_job_lock_job_name_active;
CREATE UNIQUE INDEX uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = TRUE;

-- Step 5: Add canonical external execution columns and backfill existing rows.
ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS jobexecutionid UUID,
    ADD COLUMN IF NOT EXISTS requestedby VARCHAR(100);

UPDATE fps.job_queue
SET
    jobexecutionid = COALESCE(jobexecutionid, gen_random_uuid()),
    requestedby = COALESCE(NULLIF(requestedby, ''), 'system')
WHERE jobexecutionid IS NULL
   OR requestedby IS NULL
   OR requestedby = '';

ALTER TABLE fps.job_queue
    ALTER COLUMN jobexecutionid SET NOT NULL,
    ALTER COLUMN requestedby SET NOT NULL,
    ALTER COLUMN jobqueueid SET DEFAULT gen_random_uuid();

CREATE UNIQUE INDEX IF NOT EXISTS uq_job_queue_jobexecutionid
    ON fps.job_queue (jobexecutionid);

CREATE INDEX IF NOT EXISTS idx_job_queue_requestedby
    ON fps.job_queue (requestedby);

COMMIT;

-- Verification queries (run after migration to confirm):
-- SELECT COUNT(*) FROM fps.job_lock;                                 -- Should be 0 after cleanup
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'job_lock' AND table_schema = 'fps' ORDER BY ordinal_position;
-- SELECT column_name, column_default FROM information_schema.columns WHERE table_name = 'job_queue' AND table_schema = 'fps' AND column_name IN ('jobqueueid', 'jobexecutionid', 'requestedby');
