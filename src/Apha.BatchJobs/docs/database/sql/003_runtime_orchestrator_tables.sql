-- Runtime persistence table used by the current lock repository.
-- Execution persistence now writes to foundational tbljob* tables.

BEGIN;

CREATE SCHEMA IF NOT EXISTS operational;

CREATE TABLE IF NOT EXISTS operational.job_lock (
    lock_id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    job_name VARCHAR(255) NOT NULL,
    acquired_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    run_id VARCHAR(64) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name
    ON operational.job_lock (job_name);

CREATE INDEX IF NOT EXISTS idx_job_lock_job_name_active
    ON operational.job_lock (job_name, is_active);

CREATE INDEX IF NOT EXISTS idx_job_lock_expires_at
    ON operational.job_lock (expires_at);

DROP TABLE IF EXISTS operational.job_execution_record;

COMMIT;
