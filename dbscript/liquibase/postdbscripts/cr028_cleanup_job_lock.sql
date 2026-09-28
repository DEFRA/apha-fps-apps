--liquibase formatted sql

--changeset repo-admin:CR028_cleanup labels:dml context:all
--comment: DML extracted from changesets/cr028_add_constraint_job_queue.sql. Independent stale-lock cleanup, safe to run after all changesets.

DELETE FROM fps.job_lock
WHERE lock_expires_at_utc < NOW();

--ROLLBACK
--Not Applicable
