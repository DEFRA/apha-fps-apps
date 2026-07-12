--liquibase formatted sql

--changeset repo-admin:CR028 labels:ddl context:all

ALTER TABLE fps.job_queue
DROP CONSTRAINT IF EXISTS chk_job_queue_ended_at_utc_after_start;

ALTER TABLE fps.job_queue
ADD CONSTRAINT chk_job_queue_ended_at_utc_after_start CHECK (
    ended_at_utc IS NULL OR ended_at_utc >= startdatetime
);

--changeset repo-admin:CR028_cleanup labels:dml context:all
DELETE FROM fps.job_lock
WHERE lock_expires_at_utc < NOW();

--ROLLBACK
--Not Applicable