--liquibase formatted sql

--changeset repo-admin:CR047_seed labels:dml context:all
--comment: DML extracted from changesets/cr047_milestone_notifications_job_and_audit.sql. Job seed data, independent of the audit tables created in that changeset.

INSERT INTO fps.job_master
    (jobname, frequency, timetolive, created_at, updated_at)
SELECT
    'MilestoneUpdateNotifications',
    'Scheduled',
    20,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM fps.job_master
    WHERE jobname = 'MilestoneUpdateNotifications'
);

INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, s.status
FROM fps.job_master jm
CROSS JOIN (
    VALUES ('Initiated'), ('Running'), ('Completed'), ('Failed')
) AS s(status)
WHERE jm.jobname = 'MilestoneUpdateNotifications'
AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
      AND js.status = s.status
);

--ROLLBACK
--Not Applicable
