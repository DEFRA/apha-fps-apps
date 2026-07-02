-- CR024 (DB): RecreateSummary lock contention and CreateTimeCostCalcs performance hardening.
-- NOTE: Uses non-concurrent index creation because key source tables are partitioned.

-- 1) Enforce only one active lock row per job name.
CREATE UNIQUE INDEX IF NOT EXISTS uq_job_lock_job_name_active
    ON fps.job_lock (job_name)
    WHERE is_active = true;

-- 2) Speed up active execution lookup for same job + year guard.
CREATE INDEX IF NOT EXISTS idx_job_queue_jobid_fpsyear_statusid
    ON fps.job_queue (jobid, fpsyear, statusid, updated_at DESC);

-- 3) Speed up CreateTimeCostCalcs join path.
CREATE INDEX IF NOT EXISTS idx_monthlytime_fpsyear_join
    ON fps.monthlytime (fpsyear, pactstaffid, workgroup, timecode, parentproject, month);

CREATE INDEX IF NOT EXISTS idx_timecodevalid_fpsyear_join
    ON fps.timecodevalid (fpsyear, workgroup, timecode, parentproject);

CREATE INDEX IF NOT EXISTS idx_tlkpproject_fpsyear_parentproject
    ON fps.tlkpproject (fpsyear, parentproject);

CREATE INDEX IF NOT EXISTS idx_tlkpprogram_fpsyear_programno
    ON fps.tlkpprogram (fpsyear, programno);

CREATE INDEX IF NOT EXISTS idx_workgroupgrade_fpsyear_wggrade
    ON fps.workgroupgrade (fpsyear, wggrade, profitcentregrade);

CREATE INDEX IF NOT EXISTS idx_profitcentregrade_fpsyear_pcgrade
    ON fps.profitcentregrade (fpsyear, pcgrade, profitcentre);

-- Refresh planner stats after index creation.
ANALYZE fps.job_lock;
ANALYZE fps.job_queue;
ANALYZE fps.monthlytime;
ANALYZE fps.timecodevalid;
ANALYZE fps.tlkpproject;
ANALYZE fps.tlkpprogram;
ANALYZE fps.workgroupgrade;
ANALYZE fps.profitcentregrade;
