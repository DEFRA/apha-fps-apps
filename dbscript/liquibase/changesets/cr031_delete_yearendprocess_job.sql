--liquibase formatted sql

--changeset repo-admin:CR031 labels:ddl context:all

DELETE FROM fps.job_status
WHERE jobid = (SELECT jobid FROM fps.job_master WHERE jobname = 'YearEndProcess');

DELETE FROM fps.job_master
WHERE jobname = 'YearEndProcess';

--rollback INSERT INTO fps.job_master (jobname, frequency, note, timetolive, created_at, updated_at) VALUES ('YearEndProcess', 'Manual', 'YearEnd Process', 3600, NOW(), NOW()) ON CONFLICT (jobname) DO NOTHING;
--rollback INSERT INTO fps.job_status (jobid, status, created_at) SELECT jm.jobid, s.status_value, NOW() FROM fps.job_master jm CROSS JOIN (VALUES ('Initiated'), ('Running'), ('Completed'), ('Failed'), ('Cancelled')) AS s(status_value) WHERE jm.jobname = 'YearEndProcess' ON CONFLICT (jobid, status) DO NOTHING;
