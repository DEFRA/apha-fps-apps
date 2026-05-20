-- Migration: Transition from internally-generated RunId to caller-injected JobQueueId (strict mode)
-- 
-- Changes:
-- 1. Update job_lock table schema: replace run_id (varchar) with jobqueueid (UUID)
-- 2. Enforce strict mode: caller must always provide JobQueueId
-- 3. Remove DEFAULT gen_random_uuid() from job_queue table (enforce explicit caller injection)
--
-- Backward compatibility: 
-- - Existing locks in job_lock will be cleared during deployment (assumed not critical for migrations)
-- - Future inserts MUST provide explicit jobqueueid; application fails fast if missing

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
DROP INDEX IF EXISTS uq_job_lock_job_name_active;
CREATE UNIQUE INDEX uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = TRUE;

-- Step 5: Ensure job_queue.jobqueueid has no DEFAULT (caller must inject)
-- Note: This requires manual DDL review in production; EF Core handles application-layer enforcement
ALTER TABLE fps.job_queue
    ALTER COLUMN jobqueueid DROP DEFAULT IF EXISTS;

COMMIT;

-- Verification queries (run after migration to confirm):
-- SELECT COUNT(*) FROM fps.job_lock;                                 -- Should be 0 after cleanup
-- SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'job_lock' AND table_schema = 'fps' ORDER BY ordinal_position;
-- SELECT column_name, column_default FROM information_schema.columns WHERE table_name = 'job_queue' AND table_schema = 'fps' AND column_name = 'jobqueueid';
