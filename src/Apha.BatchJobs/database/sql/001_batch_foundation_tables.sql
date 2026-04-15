-- Foundation schema for batch-job orchestration tables.
-- Safe to re-run: all CREATE statements use IF NOT EXISTS where possible.

BEGIN;

-- For gen_random_uuid() default values.
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE SCHEMA IF NOT EXISTS operational;

-- Master table for batch job definitions.
CREATE TABLE IF NOT EXISTS operational.tbljobmaster (
    jobid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobname VARCHAR(100) NOT NULL UNIQUE,
    frequency VARCHAR(50),
    note VARCHAR(250),
    timetolive INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_tbljobmaster_timetolive_positive CHECK (timetolive > 0)
);

-- Reference table for statuses used by each job.
CREATE TABLE IF NOT EXISTS operational.tbljobstatus (
    statusid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobid INTEGER NOT NULL,
    status VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_tbljobstatus_jobid
        FOREIGN KEY (jobid)
        REFERENCES operational.tbljobmaster(jobid)
        ON DELETE CASCADE,
    CONSTRAINT uq_tbljobstatus_jobid_status UNIQUE (jobid, status)
);

-- Queue table that represents one execution instance of a job.
CREATE TABLE IF NOT EXISTS operational.tbljobqueue (
    jobqueueid UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    jobid INTEGER NOT NULL,
    statusid INTEGER NOT NULL,
    startdatetime TIMESTAMPTZ NOT NULL,
    enddatetime TIMESTAMPTZ,
    errormessage VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_tbljobqueue_jobid
        FOREIGN KEY (jobid)
        REFERENCES operational.tbljobmaster(jobid)
        ON DELETE RESTRICT,
    CONSTRAINT fk_tbljobqueue_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.tbljobstatus(statusid)
        ON DELETE RESTRICT,
    CONSTRAINT chk_tbljobqueue_end_after_start CHECK (
        enddatetime IS NULL OR enddatetime >= startdatetime
    )
);

-- Detailed chronological log entries for each job queue item.
CREATE TABLE IF NOT EXISTS operational.tbljobqueue_log (
    jobqueuelogid INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    jobqueueid UUID NOT NULL,
    statusid INTEGER NOT NULL,
    performedby VARCHAR(100) NOT NULL,
    logtime TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    note VARCHAR(500),
    CONSTRAINT fk_tbljobqueue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES operational.tbljobqueue(jobqueueid)
        ON DELETE CASCADE,
    CONSTRAINT fk_tbljobqueue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES operational.tbljobstatus(statusid)
        ON DELETE RESTRICT
);

-- Useful indexes for common orchestration and monitoring lookups.
CREATE INDEX IF NOT EXISTS idx_tbljobqueue_jobid_startdatetime
    ON operational.tbljobqueue (jobid, startdatetime DESC);

CREATE INDEX IF NOT EXISTS idx_tbljobqueue_statusid
    ON operational.tbljobqueue (statusid);

CREATE INDEX IF NOT EXISTS idx_tbljobqueue_log_jobqueueid_logtime
    ON operational.tbljobqueue_log (jobqueueid, logtime DESC);

CREATE INDEX IF NOT EXISTS idx_tbljobstatus_jobid
    ON operational.tbljobstatus (jobid);

COMMENT ON TABLE operational.tbljobmaster IS
    'Batch job definitions and runtime metadata.';
COMMENT ON TABLE operational.tbljobstatus IS
    'Allowed statuses per job definition.';
COMMENT ON TABLE operational.tbljobqueue IS
    'One row per job execution instance.';
COMMENT ON TABLE operational.tbljobqueue_log IS
    'Chronological audit trail for each execution instance.';

COMMIT;
