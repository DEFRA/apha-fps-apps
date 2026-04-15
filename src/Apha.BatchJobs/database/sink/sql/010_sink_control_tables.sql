-- Sink control and metadata tables.
-- Purpose: track cloud snapshot imports and curated/sync runs.

BEGIN;

CREATE SCHEMA IF NOT EXISTS sink_meta;
CREATE SCHEMA IF NOT EXISTS sink_raw;
CREATE SCHEMA IF NOT EXISTS sink_curated;

CREATE TABLE IF NOT EXISTS sink_meta.snapshot_manifest (
    snapshot_id UUID PRIMARY KEY,
    source_environment VARCHAR(100) NOT NULL,
    source_host VARCHAR(255) NOT NULL,
    source_database VARCHAR(128) NOT NULL,
    dump_file_name VARCHAR(255) NOT NULL,
    dump_sha256 VARCHAR(128),
    imported_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    imported_by VARCHAR(100) NOT NULL,
    note VARCHAR(500)
);

CREATE TABLE IF NOT EXISTS sink_meta.pipeline_run (
    run_id UUID PRIMARY KEY,
    snapshot_id UUID NOT NULL,
    stage VARCHAR(50) NOT NULL,
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    status VARCHAR(20) NOT NULL,
    rows_read BIGINT,
    rows_written BIGINT,
    error_text TEXT,
    CONSTRAINT fk_pipeline_run_snapshot
        FOREIGN KEY (snapshot_id)
        REFERENCES sink_meta.snapshot_manifest(snapshot_id)
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_pipeline_run_snapshot_stage
    ON sink_meta.pipeline_run (snapshot_id, stage, started_at DESC);

COMMIT;
