--liquibase formatted sql

--changeset repo-admin:CR046 labels:ddl context:all splitStatements:false

ALTER TABLE fps.job_queue
    ALTER COLUMN startdatetime DROP NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'uq_job_status_jobid_statusid'
          AND conrelid = 'fps.job_status'::regclass
    ) THEN
        ALTER TABLE fps.job_status
            ADD CONSTRAINT uq_job_status_jobid_statusid
            UNIQUE (jobid, statusid);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_job_queue_job_status'
          AND conrelid = 'fps.job_queue'::regclass
    ) THEN
        ALTER TABLE fps.job_queue
            ADD CONSTRAINT fk_job_queue_job_status
            FOREIGN KEY (jobid, statusid)
            REFERENCES fps.job_status (jobid, statusid);
    END IF;
END $$;

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS cancelled_by varchar(100),
    ADD COLUMN IF NOT EXISTS cancelled_at_utc timestamptz,
    ADD COLUMN IF NOT EXISTS cancellation_reason varchar(500);

ALTER TABLE fps.job_queue
    DROP COLUMN IF EXISTS ended_at_utc,
    DROP COLUMN IF EXISTS updated_at_utc;

ALTER TABLE fps.job_lock
    DROP COLUMN IF EXISTS lock_expires_at_utc;

--rollback ALTER TABLE fps.job_lock ADD COLUMN IF NOT EXISTS lock_expires_at_utc timestamptz;
--rollback ALTER TABLE fps.job_queue ADD COLUMN IF NOT EXISTS updated_at_utc timestamptz;
--rollback ALTER TABLE fps.job_queue ADD COLUMN IF NOT EXISTS ended_at_utc timestamptz;
--rollback ALTER TABLE fps.job_queue DROP COLUMN IF EXISTS cancellation_reason;
--rollback ALTER TABLE fps.job_queue DROP COLUMN IF EXISTS cancelled_at_utc;
--rollback ALTER TABLE fps.job_queue DROP COLUMN IF EXISTS cancelled_by;
--rollback ALTER TABLE fps.job_queue DROP CONSTRAINT IF EXISTS fk_job_queue_job_status;
--rollback ALTER TABLE fps.job_status DROP CONSTRAINT IF EXISTS uq_job_status_jobid_statusid;
--rollback ALTER TABLE fps.job_queue ALTER COLUMN startdatetime SET NOT NULL;
