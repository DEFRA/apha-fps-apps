--liquibase formatted sql

--changeset repo-admin:CR018 labels:ddl context:all

ALTER TABLE fps.job_queue ADD COLUMN fpsyear INTEGER NULL;

CREATE INDEX idx_job_queue_jobid_fpsyear_requested ON fps.job_queue(jobid, fpsyear, requested_at_utc DESC); --WHERE fpsyear IS NOT NULL;

--ROLLBACK
--Not Applicable