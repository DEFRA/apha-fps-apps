--liquibase formatted sql

--changeset repo-admin:CR017 labels:ddl context:all

BEGIN;

-- Remove Cancelled status from seed data (deferred per 2026-06-23 decision)
DELETE FROM fps.job_status
WHERE status_name = 'Cancelled';

-- Ensure the 4 active states exist (idempotent upsert)
INSERT INTO fps.job_status (status_name)
VALUES ('Initiated'), ('Running'), ('Completed'), ('Failed')
ON CONFLICT (status_name) DO NOTHING;

COMMIT;

--ROLLBACK
--Not Applicable