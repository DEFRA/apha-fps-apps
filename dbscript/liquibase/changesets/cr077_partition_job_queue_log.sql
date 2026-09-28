--liquibase formatted sql

--changeset repo-admin:CR077 labels:ddl context:all splitStatements:false

BEGIN;

-- Proposed new CR - fps.job_queue_log FPS-year retention partitioning.
-- fps.job_queue remains unpartitioned; FK remains jobqueueid-only.

ALTER TABLE fps.job_queue_log
    ADD COLUMN IF NOT EXISTS fpsyear integer;

-- Backfill:
-- * use job_queue.fpsyear when the execution is explicitly year-scoped;
-- * otherwise derive FPS year from logtime using Apr-Mar:
--   Apr YYYY-Mar YYYY+1 => fpsyear YYYY.
UPDATE fps.job_queue_log jql
SET fpsyear = COALESCE(
    jq.fpsyear,
    CASE
        WHEN EXTRACT(MONTH FROM jql.logtime AT TIME ZONE 'UTC') >= 4
            THEN EXTRACT(YEAR FROM jql.logtime AT TIME ZONE 'UTC')::integer
        ELSE EXTRACT(YEAR FROM jql.logtime AT TIME ZONE 'UTC')::integer - 1
    END
)
FROM fps.job_queue jq
WHERE jq.jobqueueid = jql.jobqueueid
  AND jql.fpsyear IS NULL;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM fps.job_queue_log WHERE fpsyear IS NULL) THEN
        RAISE EXCEPTION
            'job_queue_log migration blocked: unresolved NULL fpsyear rows remain.';
    END IF;

    IF to_regclass('fps.job_queue_log_p') IS NOT NULL
       OR to_regclass('fps.job_queue_log_old') IS NOT NULL THEN
        RAISE EXCEPTION
            'job_queue_log migration precondition failed: working/backup table already exists.';
    END IF;
END
$$;

ALTER TABLE fps.job_queue_log
    ALTER COLUMN fpsyear SET NOT NULL;

CREATE TABLE fps.job_queue_log_p
    (LIKE fps.job_queue_log INCLUDING ALL EXCLUDING INDEXES)
    PARTITION BY LIST (fpsyear);

ALTER TABLE fps.job_queue_log_p
    ADD CONSTRAINT job_queue_log_p_pkey
        PRIMARY KEY (jobqueuelogid, fpsyear),
    ADD CONSTRAINT fk_job_queue_log_jobqueueid
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue(jobqueueid)
        ON DELETE CASCADE,
    ADD CONSTRAINT fk_job_queue_log_statusid
        FOREIGN KEY (statusid)
        REFERENCES fps.job_status(statusid)
        ON DELETE RESTRICT;

DO $$
DECLARE y integer;
BEGIN
    FOR y IN 2016..2027 LOOP
        EXECUTE format(
            'CREATE TABLE fps.job_queue_log_p_y%s PARTITION OF fps.job_queue_log_p FOR VALUES IN (%s)',
            y, y
        );
    END LOOP;
END
$$;

CREATE TABLE fps.job_queue_log_p_default
    PARTITION OF fps.job_queue_log_p DEFAULT;

INSERT INTO fps.job_queue_log_p
    OVERRIDING SYSTEM VALUE
SELECT * FROM fps.job_queue_log;

ALTER TABLE fps.job_queue_log
    RENAME TO job_queue_log_old;
ALTER TABLE fps.job_queue_log_p
    RENAME TO job_queue_log;

DROP INDEX IF EXISTS fps.idx_job_queue_log_jobqueueid_logtime;
CREATE INDEX idx_job_queue_log_jobqueueid_logtime
    ON fps.job_queue_log (jobqueueid, logtime DESC);

SELECT setval(
    pg_get_serial_sequence('fps.job_queue_log', 'jobqueuelogid'),
    GREATEST(COALESCE((SELECT MAX(jobqueuelogid) FROM fps.job_queue_log), 0), 1),
    COALESCE((SELECT MAX(jobqueuelogid) FROM fps.job_queue_log), 0) > 0
);

COMMIT;

-- Rollback restores the pre-CR077 table retained as job_queue_log_old.
-- Rows written to the partitioned table after CR077 are discarded.
--rollback LOCK TABLE fps.job_queue_log, fps.job_queue_log_old IN ACCESS EXCLUSIVE MODE;
--rollback ALTER TABLE fps.job_queue_log RENAME TO job_queue_log_partitioned;
--rollback ALTER TABLE fps.job_queue_log_old RENAME TO job_queue_log;
--rollback ALTER TABLE fps.job_queue_log DROP COLUMN fpsyear;
--rollback DROP TABLE fps.job_queue_log_partitioned CASCADE;
--rollback CREATE INDEX idx_job_queue_log_jobqueueid_logtime ON fps.job_queue_log (jobqueueid, logtime DESC);

-- Application deployment required with this CR:
-- * add FpsYear to TblJobQueueLog;
-- * populate it at all 4 known write paths;
-- * year-scoped jobs use job_queue.fpsyear;
-- * non-year-scoped jobs derive the same Apr-Mar FPS year;
-- * EF key mapping becomes (jobqueuelogid, fpsyear).