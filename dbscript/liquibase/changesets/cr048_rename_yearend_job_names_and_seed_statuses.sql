--liquibase formatted sql

--changeset repo-admin:CR048 labels:ddl context:all splitStatements:false

-- CR048 depends on CR030 having created the original Year End job rows.
-- Preserve the existing jobid values and all related job_queue/job_status history.

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEndDataSetup'
    )
    AND EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEnd-DataSetup'
    ) THEN
        RAISE EXCEPTION
            'Both YearEndDataSetup and YearEnd-DataSetup exist. Resolve duplicate job catalogue rows before applying CR048.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEndCutover'
    )
    AND EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEnd-CutOver'
    ) THEN
        RAISE EXCEPTION
            'Both YearEndCutover and YearEnd-CutOver exist. Resolve duplicate job catalogue rows before applying CR048.';
    END IF;
END $$;

UPDATE fps.job_master
SET jobname = 'YearEnd-DataSetup',
    updated_at = NOW()
WHERE jobname = 'YearEndDataSetup';

UPDATE fps.job_master
SET jobname = 'YearEnd-CutOver',
    updated_at = NOW()
WHERE jobname = 'YearEndCutover';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEnd-DataSetup'
    ) THEN
        RAISE EXCEPTION
            'YearEnd-DataSetup job row was not found after rename. Confirm CR030 has been applied.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM fps.job_master
        WHERE jobname = 'YearEnd-CutOver'
    ) THEN
        RAISE EXCEPTION
            'YearEnd-CutOver job row was not found after rename. Confirm CR030 has been applied.';
    END IF;
END $$;

-- Ensure both jobs have the complete seven-state lifecycle.
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
WHERE jm.jobname IN (
    'YearEnd-DataSetup',
    'YearEnd-CutOver'
)
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
-- WHERE jm.jobname IN (
--     'YearEnd-DataSetup',
--     'YearEnd-CutOver'
-- )
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

--rollback UPDATE fps.job_master SET jobname = 'YearEndCutover', updated_at = NOW() WHERE jobname = 'YearEnd-CutOver';
--rollback UPDATE fps.job_master SET jobname = 'YearEndDataSetup', updated_at = NOW() WHERE jobname = 'YearEnd-DataSetup';
