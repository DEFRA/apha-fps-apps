--liquibase formatted sql

--changeset repo-admin:CR017 labels:ddl context:all

BEGIN;

-- Remove Cancelled status from seed data (deferred per 2026-06-23 decision)
DELETE FROM fps.job_status
WHERE status = 'Cancelled';

COMMIT;

--ROLLBACK
--Not Applicable