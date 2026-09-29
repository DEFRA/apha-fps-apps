--liquibase formatted sql

--changeset repo-admin:CR044_seed labels:dml context:all
--comment: DML extracted from changesets/cr044_bulk_rates_jobs_staging_and_validation.sql. Job seed data, independent of the staging tables created in that changeset.

INSERT INTO fps.job_master
    (jobname, frequency, timetolive, created_at, updated_at)
SELECT v.jobname, 'Manual', 20, NOW(), NOW()
FROM (
    VALUES
        ('BulkTestRatesUpdate'),
        ('BulkStaffRatesUpdate'),
        ('BulkAnimalRatesUpdate')
) AS v(jobname)
WHERE NOT EXISTS (
    SELECT 1
    FROM fps.job_master jm
    WHERE jm.jobname = v.jobname
);

INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, s.status
FROM fps.job_master jm
CROSS JOIN (
    VALUES
        ('Initiated'),
        ('ReleasedForApproval'),
        ('Approved'),
        ('Rejected'),
        ('Running'),
        ('Completed'),
        ('Failed'),
        ('Cancelled')
) AS s(status)
WHERE jm.jobname IN (
    'BulkTestRatesUpdate',
    'BulkStaffRatesUpdate',
    'BulkAnimalRatesUpdate'
)
AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
      AND js.status = s.status
);

--ROLLBACK
--Not Applicable
