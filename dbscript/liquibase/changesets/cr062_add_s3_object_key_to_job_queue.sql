--liquibase formatted sql

--changeset repo-admin:CR062 labels:ddl context:all

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS s3_object_key text;

--ROLLBACK
--ALTER TABLE fps.job_queue DROP COLUMN IF EXISTS s3_object_key;