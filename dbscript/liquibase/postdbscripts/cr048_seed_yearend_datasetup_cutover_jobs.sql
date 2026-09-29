--liquibase formatted sql

--changeset repo-admin:CR048 labels:ddl context:all

INSERT INTO fps.job_master (jobname, frequency, note, timetolive)
SELECT 'YearEnd-DataSetup', NULL, NULL, 1
WHERE NOT EXISTS (SELECT 1 FROM fps.job_master WHERE jobname = 'YearEnd-DataSetup');

INSERT INTO fps.job_master (jobname, frequency, note, timetolive)
SELECT 'YearEnd-CutOver', NULL, NULL, 1
WHERE NOT EXISTS (SELECT 1 FROM fps.job_master WHERE jobname = 'YearEnd-CutOver');

INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, s.status
FROM fps.job_master jm
CROSS JOIN (
    VALUES
        ('Initiated'),
        ('Approved'),
        ('Rejected'),
        ('Running'),
        ('Failed'),
        ('Completed'),
        ('Cancelled')
) AS s(status)
WHERE jm.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver')
AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
      AND js.status = s.status
);

-- Verification (run manually after deployment):
-- SELECT
--     jm.jobid,
--     jm.jobname,
--     js.status
-- FROM fps.job_master jm
-- JOIN fps.job_status js
--   ON js.jobid = jm.jobid
-- WHERE jm.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver')
-- ORDER BY
--     jm.jobname,
--     CASE js.status
--         WHEN 'Initiated' THEN 1
--         WHEN 'Approved' THEN 2
--         WHEN 'Rejected' THEN 3
--         WHEN 'Running' THEN 4
--         WHEN 'Failed' THEN 5
--         WHEN 'Completed' THEN 6
--         WHEN 'Cancelled' THEN 7
--         ELSE 99
--     END;

--rollback DELETE FROM fps.job_status WHERE jobid IN (SELECT jobid FROM fps.job_master WHERE jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver'));
--rollback DELETE FROM fps.job_master WHERE jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver');
