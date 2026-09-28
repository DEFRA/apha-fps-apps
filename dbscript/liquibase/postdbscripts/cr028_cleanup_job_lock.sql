--liquibase formatted sql

--changeset repo-admin:CR028_cleanup labels:dml context:all
--comment: DML extracted from changesets/cr028_add_constraint_job_queue.sql. Independent stale-lock cleanup, safe to run after all changesets.

DELETE FROM fps.job_lock
WHERE expires_at < NOW();

--ROLLBACK
--Not Applicable
