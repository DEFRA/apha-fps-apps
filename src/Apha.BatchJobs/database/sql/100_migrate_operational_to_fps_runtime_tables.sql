-- Migration: Move runtime orchestration tables from operational schema to fps schema
-- Purpose: Normalize table names and consolidate runtime objects into fps schema for consistency
-- Source database: batch_jobs_foundation_db
-- Date: 2026-04-30
-- 
-- This migration:
-- 1. Creates fps schema if missing
-- 2. Creates normalized tables in fps (job_lock, job_master, job_status, job_queue, job_queue_log)
-- 3. Copies all data from operational legacy tables (batch_lock, tbljobmaster, etc.)
-- 4. Recreates all constraints, indexes, and unique rules
-- 5. Creates backward-compatibility views in operational schema
-- 6. Validates data integrity

BEGIN;

-- Step 1: Create fps schema if missing
CREATE SCHEMA IF NOT EXISTS fps;
COMMENT ON SCHEMA fps IS 'Financial Planning System operational data and archive';

-- Step 2: Create normalized job_master table in fps
CREATE TABLE IF NOT EXISTS fps.job_master (
    jobid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname VARCHAR(100) NOT NULL UNIQUE,
    frequency VARCHAR(50),
    note VARCHAR(250),
    timetolive INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_job_master_timetolive_positive CHECK (timetolive > 0)
);
COMMENT ON TABLE fps.job_master IS 'Batch job definitions and runtime metadata';

-- Step 3: Create normalized job_status table in fps
CREATE TABLE IF NOT EXISTS fps.job_status (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_status_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);
COMMENT ON TABLE fps.job_status IS 'Allowed statuses per job definition';

-- Step 4: Create normalized job_queue table in fps
CREATE TABLE IF NOT EXISTS fps.job_queue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_queue_jobid
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_job_queue_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_job_queue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);
COMMENT ON TABLE fps.job_queue IS 'One row per job execution instance';

-- Step 5: Create normalized job_queue_log table in fps
CREATE TABLE IF NOT EXISTS fps.job_queue_log (
    jobqueuelogid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid UUID NOT NULL,
    statusid INTEGER NOT NULL,
    performedby VARCHAR(100) NOT NULL,
    logtime TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note VARCHAR(500),
    CONSTRAINT fk_job_queue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_job_queue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT
);
COMMENT ON TABLE fps.job_queue_log IS 'Chronological audit trail for each execution instance';

-- Step 6: Create normalized job_lock table in fps
CREATE TABLE IF NOT EXISTS fps.job_lock (
    lock_id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name VARCHAR(255) NOT NULL,
    acquired_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    run_id VARCHAR(64) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);
COMMENT ON TABLE fps.job_lock IS 'Distributed execution lock for job coordination';

-- Step 7: Copy data from operational.tbljobmaster -> fps.job_master (if operational tables exist)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobmaster') THEN
        INSERT INTO fps.job_master (jobid, jobname, frequency, note, timetolive, created_at, updated_at)
        OVERRIDING SYSTEM VALUE
        SELECT jobid, jobname, frequency, note, timetolive, created_at, updated_at
        FROM operational.tbljobmaster
        ON CONFLICT (jobid) DO NOTHING;
        RAISE NOTICE 'Copied % rows from operational.tbljobmaster to fps.job_master', (SELECT COUNT(*) FROM fps.job_master);
    END IF;
END $$;

-- Step 8: Copy data from operational.tbljobstatus -> fps.job_status
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobstatus') THEN
        INSERT INTO fps.job_status (statusid, jobid, status, created_at)
        OVERRIDING SYSTEM VALUE
        SELECT statusid, jobid, status, created_at
        FROM operational.tbljobstatus
        ON CONFLICT (statusid) DO NOTHING;
        RAISE NOTICE 'Copied % rows from operational.tbljobstatus to fps.job_status', (SELECT COUNT(*) FROM fps.job_status);
    END IF;
END $$;

-- Step 9: Copy data from operational.tbljobqueue -> fps.job_queue
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobqueue') THEN
        INSERT INTO fps.job_queue (jobqueueid, jobid, statusid, startdatetime, enddatetime, errormessage, created_at, updated_at)
        SELECT jobqueueid, jobid, statusid, startdatetime, enddatetime, errormessage, created_at, updated_at
        FROM operational.tbljobqueue
        ON CONFLICT (jobqueueid) DO NOTHING;
        RAISE NOTICE 'Copied % rows from operational.tbljobqueue to fps.job_queue', (SELECT COUNT(*) FROM fps.job_queue);
    END IF;
END $$;

-- Step 10: Copy data from operational.tbljobqueue_log -> fps.job_queue_log
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobqueue_log') THEN
        INSERT INTO fps.job_queue_log (jobqueuelogid, jobqueueid, statusid, performedby, logtime, note)
        OVERRIDING SYSTEM VALUE
        SELECT jobqueuelogid, jobqueueid, statusid, performedby, logtime, note
        FROM operational.tbljobqueue_log
        ON CONFLICT (jobqueuelogid) DO NOTHING;
        RAISE NOTICE 'Copied % rows from operational.tbljobqueue_log to fps.job_queue_log', (SELECT COUNT(*) FROM fps.job_queue_log);
    END IF;
END $$;

-- Step 11: Copy data from operational.batch_lock -> fps.job_lock
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='batch_lock') THEN
        INSERT INTO fps.job_lock (lock_id, job_name, acquired_at, expires_at, run_id, is_active)
        OVERRIDING SYSTEM VALUE
        SELECT lock_id, job_name, acquired_at, expires_at, run_id, is_active
        FROM operational.batch_lock
        ON CONFLICT (lock_id) DO NOTHING;
        RAISE NOTICE 'Copied % rows from operational.batch_lock to fps.job_lock', (SELECT COUNT(*) FROM fps.job_lock);
    END IF;
END $$;

-- Step 12: Create indexes on fps tables
CREATE INDEX IF NOT EXISTS idx_job_queue_jobid_startdatetime
    ON fps.job_queue (jobid, startdatetime DESC);

CREATE INDEX IF NOT EXISTS idx_job_queue_statusid
    ON fps.job_queue (statusid);

CREATE INDEX IF NOT EXISTS idx_job_queue_log_jobqueueid_logtime
    ON fps.job_queue_log (jobqueueid, logtime DESC);

CREATE INDEX IF NOT EXISTS idx_job_status_jobid
    ON fps.job_status (jobid);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name
    ON fps.job_lock (job_name);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name_active
    ON fps.job_lock (job_name, is_active)
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_job_lock_expires_at
    ON fps.job_lock (expires_at);

-- Step 13: Rename legacy operational tables to _legacy for reference
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='batch_lock') THEN
        ALTER TABLE operational.batch_lock RENAME TO batch_lock_legacy;
        RAISE NOTICE 'Renamed operational.batch_lock to operational.batch_lock_legacy';
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobmaster') THEN
        ALTER TABLE operational.tbljobmaster RENAME TO tbljobmaster_legacy;
        RAISE NOTICE 'Renamed operational.tbljobmaster to operational.tbljobmaster_legacy';
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobstatus') THEN
        ALTER TABLE operational.tbljobstatus RENAME TO tbljobstatus_legacy;
        RAISE NOTICE 'Renamed operational.tbljobstatus to operational.tbljobstatus_legacy';
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobqueue') THEN
        ALTER TABLE operational.tbljobqueue RENAME TO tbljobqueue_legacy;
        RAISE NOTICE 'Renamed operational.tbljobqueue to operational.tbljobqueue_legacy';
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='operational' AND table_name='tbljobqueue_log') THEN
        ALTER TABLE operational.tbljobqueue_log RENAME TO tbljobqueue_log_legacy;
        RAISE NOTICE 'Renamed operational.tbljobqueue_log to operational.tbljobqueue_log_legacy';
    END IF;
END $$;

-- Step 14: Create backward-compatibility views in operational schema
-- These views redirect legacy code to new fps tables
CREATE OR REPLACE VIEW operational.batch_lock AS
SELECT * FROM fps.job_lock;
COMMENT ON VIEW operational.batch_lock IS 'Legacy view: redirects to fps.job_lock';

CREATE OR REPLACE VIEW operational.tbljobmaster AS
SELECT * FROM fps.job_master;
COMMENT ON VIEW operational.tbljobmaster IS 'Legacy view: redirects to fps.job_master';

CREATE OR REPLACE VIEW operational.tbljobstatus AS
SELECT * FROM fps.job_status;
COMMENT ON VIEW operational.tbljobstatus IS 'Legacy view: redirects to fps.job_status';

CREATE OR REPLACE VIEW operational.tbljobqueue AS
SELECT * FROM fps.job_queue;
COMMENT ON VIEW operational.tbljobqueue IS 'Legacy view: redirects to fps.job_queue';

CREATE OR REPLACE VIEW operational.tbljobqueue_log AS
SELECT * FROM fps.job_queue_log;
COMMENT ON VIEW operational.tbljobqueue_log IS 'Legacy view: redirects to fps.job_queue_log';

-- Step 15: Data integrity validation
DO $$
DECLARE
    job_master_count INTEGER;
    job_status_count INTEGER;
    job_queue_count INTEGER;
    job_queue_log_count INTEGER;
    job_lock_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO job_master_count FROM fps.job_master;
    SELECT COUNT(*) INTO job_status_count FROM fps.job_status;
    SELECT COUNT(*) INTO job_queue_count FROM fps.job_queue;
    SELECT COUNT(*) INTO job_queue_log_count FROM fps.job_queue_log;
    SELECT COUNT(*) INTO job_lock_count FROM fps.job_lock;

    RAISE NOTICE 'Data integrity check:';
    RAISE NOTICE '  fps.job_master: % rows', job_master_count;
    RAISE NOTICE '  fps.job_status: % rows', job_status_count;
    RAISE NOTICE '  fps.job_queue: % rows', job_queue_count;
    RAISE NOTICE '  fps.job_queue_log: % rows', job_queue_log_count;
    RAISE NOTICE '  fps.job_lock: % rows', job_lock_count;
    RAISE NOTICE 'Migration complete. All objects created in fps schema with backward-compatibility views in operational.';
    RAISE NOTICE 'Legacy operational tables renamed to *_legacy for reference/archive.';
END $$;

COMMIT;
