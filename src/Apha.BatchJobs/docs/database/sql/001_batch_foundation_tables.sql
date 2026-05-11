-- Foundation schema for batch-job orchestration tables.
-- Safe to re-run: all CREATE statements use IF NOT EXISTS where possible.

BEGIN;

-- For gen_random_uuid() default values.
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS operational;

-- Master table for batch job definitions.
CREATE TABLE IF NOT EXISTS operational.job_master (
    jobid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname VARCHAR(100) NOT NULL UNIQUE,
    frequency VARCHAR(50),
    note VARCHAR(250),
    timetolive INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_job_master_timetolive_positive CHECK (timetolive > 0)
);

-- Reference table for statuses used by each job.
CREATE TABLE IF NOT EXISTS operational.job_status (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_job_status_jobid
        FOREIGN KEY (jobid)
        REFERENCES operational.job_master(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_job_status_jobid_status UNIQUE (jobid, status)
);

-- Queue table that represents one execution instance of a job.
CREATE TABLE IF NOT EXISTS operational.job_queue (
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
        REFERENCES operational.job_master(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_job_queue_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.job_status(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_job_queue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);

-- Detailed chronological log entries for each job queue item.
CREATE TABLE IF NOT EXISTS operational.job_queue_log (
    jobqueuelogid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid UUID NOT NULL,
    statusid INTEGER NOT NULL,
    performedby VARCHAR(100) NOT NULL,
    logtime TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note VARCHAR(500),
    CONSTRAINT fk_job_queue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES operational.job_queue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_job_queue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.job_status(statusid)
        ON DELETE RESTRICT
);

-- Useful indexes for common orchestration and monitoring lookups.
CREATE INDEX IF NOT EXISTS idx_job_queue_jobid_startdatetime
    ON operational.job_queue (jobid, startdatetime DESC);

CREATE INDEX IF NOT EXISTS idx_job_queue_statusid
    ON operational.job_queue (statusid);

CREATE INDEX IF NOT EXISTS idx_job_queue_log_jobqueueid_logtime
    ON operational.job_queue_log (jobqueueid, logtime DESC);

CREATE INDEX IF NOT EXISTS idx_job_status_jobid
    ON operational.job_status (jobid);

COMMENT ON TABLE operational.job_master IS
    'Batch job definitions and runtime metadata.';
COMMENT ON TABLE operational.job_status IS
    'Allowed statuses per job definition.';
COMMENT ON TABLE operational.job_queue IS
    'One row per job execution instance.';
COMMENT ON TABLE operational.job_queue_log IS
    'Chronological audit trail for each execution instance.';

COMMIT;
