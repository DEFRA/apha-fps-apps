--liquibase formatted sql

--changeset repo-admin:CR029 labels:ddl context:all

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS configuration_json jsonb NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS approved_by text NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS approved_at_utc timestamp NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS rejected_by text NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS rejected_at_utc timestamp NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS rejection_reason text NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS triggered_by text NULL;

ALTER TABLE fps.job_queue
ADD COLUMN IF NOT EXISTS triggered_at_utc timestamp NULL;

--ROLLBACK
--Not Applicable