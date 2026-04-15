-- Curated projection step.
-- Requires psql variable: snapshot_id (UUID string)
-- Example:
-- psql "$SINK_CONN" -v ON_ERROR_STOP=1 -v snapshot_id="7e4b9f7c-7f0d-4ecf-ab26-7482f2d8cebb" -f database/sink/sql/020_curated_from_raw_template.sql

BEGIN;

-- Curated table used as stable sync input for foundation upserts.
CREATE TABLE IF NOT EXISTS sink_curated.job_seed (
    source_snapshot_id UUID NOT NULL,
    job_name VARCHAR(100) NOT NULL,
    frequency VARCHAR(50),
    note VARCHAR(250),
    time_to_live INTEGER NOT NULL,
    PRIMARY KEY (source_snapshot_id, job_name)
);

-- Optional normalized source contract in sink_raw.
-- Replace this table with your restored snapshot source table/view if desired.
CREATE TABLE IF NOT EXISTS sink_raw.job_seed_source (
    job_name VARCHAR(100) PRIMARY KEY,
    frequency VARCHAR(50),
    note VARCHAR(250),
    time_to_live INTEGER,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

WITH run_row AS (
    INSERT INTO sink_meta.pipeline_run (
        run_id,
        snapshot_id,
        stage,
        status
    )
    VALUES (
        gen_random_uuid(),
        :'snapshot_id'::UUID,
        'curated_extract',
        'started'
    )
    RETURNING run_id
),
upsert_curated AS (
    INSERT INTO sink_curated.job_seed (
        source_snapshot_id,
        job_name,
        frequency,
        note,
        time_to_live
    )
    SELECT
        :'snapshot_id'::UUID,
        src.job_name,
        src.frequency,
        src.note,
        GREATEST(COALESCE(src.time_to_live, 3600), 1)
    FROM sink_raw.job_seed_source src
    WHERE src.is_active = TRUE
    ON CONFLICT (source_snapshot_id, job_name)
    DO UPDATE SET
        frequency = EXCLUDED.frequency,
        note = EXCLUDED.note,
        time_to_live = EXCLUDED.time_to_live
    RETURNING 1
)
UPDATE sink_meta.pipeline_run pr
SET
    completed_at = NOW(),
    status = 'succeeded',
    rows_read = (SELECT COUNT(*) FROM sink_raw.job_seed_source src WHERE src.is_active = TRUE),
    rows_written = (SELECT COUNT(*) FROM upsert_curated)
WHERE pr.run_id = (SELECT run_id FROM run_row);

COMMIT;
