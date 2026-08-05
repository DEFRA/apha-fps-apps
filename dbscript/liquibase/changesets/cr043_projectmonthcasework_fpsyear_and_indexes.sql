--liquibase formatted sql

--changeset repo-admin:CR043 labels:ddl context:all splitStatements:false

-- 1. Make RecreateSummary casework output explicitly year-aware.
ALTER TABLE fps.projectmonthcasework
    ADD COLUMN IF NOT EXISTS fpsyear integer;

UPDATE fps.projectmonthcasework pmcw
SET fpsyear = src.fpsyear
FROM (
    SELECT project, monthno, MAX(fpsyear) AS fpsyear
    FROM fps.projectmonth
    GROUP BY project, monthno
) src
WHERE pmcw.project = src.project
  AND pmcw.monthno = src.monthno
  AND pmcw.fpsyear IS NULL;

UPDATE fps.projectmonthcasework pmcw
SET fpsyear = src.fpsyear
FROM (
    SELECT project, monthno, MAX(fpsyear) AS fpsyear
    FROM fps.projectmonth2
    GROUP BY project, monthno
) src
WHERE pmcw.project = src.project
  AND pmcw.monthno = src.monthno
  AND pmcw.fpsyear IS NULL;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM fps.projectmonthcasework
        WHERE fpsyear IS NULL
    ) THEN
        RAISE EXCEPTION
            'Cannot enforce fpsyear NOT NULL because projectmonthcasework contains unresolved rows';
    END IF;
END $$;

ALTER TABLE fps.projectmonthcasework
    ALTER COLUMN fpsyear SET NOT NULL;

ALTER TABLE fps.projectmonthcasework
    DROP CONSTRAINT IF EXISTS projectmonthcasework_pk;

ALTER TABLE fps.projectmonthcasework
    ADD CONSTRAINT projectmonthcasework_pk
    PRIMARY KEY (project, monthno, fpsyear);

-- 2. Resynchronise PeriodMonthlyOutput identity sequence.
SELECT setval(
    'fps.period_monthlyoutput_id_seq',
    GREATEST(
        COALESCE((SELECT MAX(id) FROM fps.period_monthlyoutput), 0),
        1
    ),
    COALESCE((SELECT MAX(id) FROM fps.period_monthlyoutput), 0) > 0
);

-- 3. RecreateSummary lock and execution lookup indexes.
CREATE UNIQUE INDEX IF NOT EXISTS uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = true;

CREATE INDEX IF NOT EXISTS idx_job_queue_jobid_fpsyear_statusid
    ON fps.job_queue (jobid, fpsyear, statusid);

-- 4. Year-leading indexes for CreateTimeCostCalcs source joins.
CREATE INDEX IF NOT EXISTS idx_monthlytime_fpsyear_project_month
    ON fps.monthlytime (fpsyear, project, month);

CREATE INDEX IF NOT EXISTS idx_timecodevalid_fpsyear_timecode
    ON fps.timecodevalid (fpsyear, timecode);

CREATE INDEX IF NOT EXISTS idx_tlkpproject_fpsyear_parentproject
    ON fps.tlkpproject (fpsyear, parentproject);

CREATE INDEX IF NOT EXISTS idx_tlkpprogram_fpsyear_program
    ON fps.tlkpprogram (fpsyear, program);

CREATE INDEX IF NOT EXISTS idx_workgroupgrade_fpsyear_workgroup_grade
    ON fps.workgroupgrade (fpsyear, workgroup, gradecode);

CREATE INDEX IF NOT EXISTS idx_profitcentregrade_fpsyear_profitcentre_grade
    ON fps.profitcentregrade (fpsyear, profitcentre, gradecode);

--rollback DROP INDEX IF EXISTS fps.idx_profitcentregrade_fpsyear_profitcentre_grade;
--rollback DROP INDEX IF EXISTS fps.idx_workgroupgrade_fpsyear_workgroup_grade;
--rollback DROP INDEX IF EXISTS fps.idx_tlkpprogram_fpsyear_program;
--rollback DROP INDEX IF EXISTS fps.idx_tlkpproject_fpsyear_parentproject;
--rollback DROP INDEX IF EXISTS fps.idx_timecodevalid_fpsyear_timecode;
--rollback DROP INDEX IF EXISTS fps.idx_monthlytime_fpsyear_project_month;
--rollback DROP INDEX IF EXISTS fps.idx_job_queue_jobid_fpsyear_statusid;
--rollback DROP INDEX IF EXISTS fps.uq_job_lock_job_name_active;
--rollback ALTER TABLE fps.projectmonthcasework DROP CONSTRAINT IF EXISTS projectmonthcasework_pk;
--rollback ALTER TABLE fps.projectmonthcasework ALTER COLUMN fpsyear DROP NOT NULL;
--rollback ALTER TABLE fps.projectmonthcasework DROP COLUMN IF EXISTS fpsyear;
