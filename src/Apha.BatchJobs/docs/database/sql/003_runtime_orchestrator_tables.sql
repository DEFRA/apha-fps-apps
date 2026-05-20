-- Runtime persistence table used by the current lock repository.
-- Execution persistence now writes to foundational tbljob* tables.

BEGIN;

CREATE SCHEMA IF NOT EXISTS fps;

-- Create pgcrypto extension for UUID functions if needed
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS fps.job_lock (
    lock_id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name VARCHAR(255) NOT NULL,
    acquired_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    jobqueueid UUID NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name
    ON fps.job_lock (job_name);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name_active
    ON fps.job_lock (job_name, is_active);

CREATE INDEX IF NOT EXISTS idx_job_lock_expires_at
    ON fps.job_lock (expires_at);

-- Partial unique index: only one active lock per job
CREATE UNIQUE INDEX IF NOT EXISTS uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = TRUE;

DROP TABLE IF EXISTS fps.job_execution_record;

COMMIT;
