-- Curated projection template.
-- Fill this script with SELECT/INSERT statements that extract ONLY
-- the fields needed for batch-job revisions from sink_raw.*
-- into sink_curated.* tables.

BEGIN;

-- Example table for curated mapping output. Keep this minimal and purpose-driven.
CREATE TABLE IF NOT EXISTS sink_curated.job_seed (
    source_snapshot_id UUID NOT NULL,
    job_name VARCHAR(100) NOT NULL,
    frequency VARCHAR(50),
    note VARCHAR(250),
    time_to_live INTEGER NOT NULL,
    PRIMARY KEY (source_snapshot_id, job_name)
);

-- Template extraction statement. Replace source table names as needed.
-- INSERT INTO sink_curated.job_seed (source_snapshot_id, job_name, frequency, note, time_to_live)
-- SELECT
--     :snapshot_id,
--     src.job_name,
--     src.frequency,
--     src.note,
--     COALESCE(src.time_to_live, 3600)
-- FROM sink_raw.some_source_table src
-- WHERE src.is_active = true;

COMMIT;
