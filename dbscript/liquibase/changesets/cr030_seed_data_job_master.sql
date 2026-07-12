--liquibase formatted sql

--changeset repo-admin:CR030 labels:seed context:all

INSERT INTO fps.job_master (jobname, frequency, note, timetolive)
SELECT 'YearEndDataSetup', NULL, NULL, 1
WHERE NOT EXISTS (SELECT 1 FROM fps.job_master WHERE jobname = 'YearEndDataSetup');

INSERT INTO fps.job_master (jobname, frequency, note, timetolive)
SELECT 'YearEndCutover', NULL, NULL, 1
WHERE NOT EXISTS (SELECT 1 FROM fps.job_master WHERE jobname = 'YearEndCutover');

INSERT INTO fps.job_status (jobid, status)
SELECT m.jobid, s.status
FROM fps.job_master m
CROSS JOIN (VALUES ('Initiated'), ('Approved'), ('Running'), ('Completed'), ('Failed'), ('Rejected')) AS s(status)
WHERE m.jobname IN ('YearEndDataSetup', 'YearEndCutover')
  AND NOT EXISTS (
      SELECT 1 FROM fps.job_status js WHERE js.jobid = m.jobid AND js.status = s.status
  );

--ROLLBACK
--Not Applicable