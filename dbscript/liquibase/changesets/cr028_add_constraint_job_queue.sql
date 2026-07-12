--liquibase formatted sql

--changeset repo-admin:CR028 labels:ddl context:all

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint con
        INNER JOIN pg_class rel
            ON rel.oid = con.conrelid
        INNER JOIN pg_namespace nsp
            ON nsp.oid = rel.relnamespace
        WHERE con.conname = 'chk_job_queue_ended_at_utc_after_start'
          AND nsp.nspname = 'fps'
          AND rel.relname = 'job_queue'
    ) THEN
        ALTER TABLE fps.job_queue
        ADD CONSTRAINT chk_job_queue_ended_at_utc_after_start CHECK (
            ended_at_utc IS NULL OR ended_at_utc >= startdatetime
        );
    END IF;
END $$;

DELETE FROM fps.job_lock
WHERE lock_expires_at_utc < NOW();

--ROLLBACK
--Not Applicable