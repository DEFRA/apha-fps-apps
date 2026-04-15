# Sink Database Pipeline (Cloud Snapshot -> Foundation Sync)

This module provides a safe sink workflow for keeping local foundation schema revisions in sync with cloud source structure and selected data.

## Goal

- Ingest a snapshot/dump from cloud into a sink area.
- Curate only the important data needed for batch-job schema revisions.
- Sync curated data into foundation tables in schema `operational`.
- Keep all steps repeatable and auditable.

## Schemas

- `sink_raw`: raw imported objects from cloud snapshot.
- `sink_curated`: selected and transformed subset for revision work.
- `sink_meta`: metadata and pipeline run tracking.
- `operational`: foundation runtime schema used by BatchJobs.

## SQL Files

- `sql/010_sink_control_tables.sql`: creates sink schemas and metadata tracking tables.
- `sql/020_curated_from_raw_template.sql`: template for curated extraction from raw data.
- `sql/030_sync_foundation_from_curated_template.sql`: template for idempotent sync into foundation tables.

## Recommended VM Workflow

1. Connect your VM to OpenVPN and validate cloud DB reachability.
2. Create a dump from cloud DB (schema-only or selected data).
3. Restore the dump into a sink database/schema (`sink_raw`).
4. Run `010` once (or re-run safely).
5. Run `020` with your curated extraction SQL.
6. Run `030` to sync curated data to `operational`.
7. Validate row counts and audit entries in `sink_meta.pipeline_run`.
8. Commit SQL changes and push.

## Command Templates (run from VM)

### A) Apply sink control schema

```bash
psql "$SINK_CONN" -v ON_ERROR_STOP=1 -f database/sink/sql/010_sink_control_tables.sql
```

### B) Register a snapshot import

```sql
INSERT INTO sink_meta.snapshot_manifest (
    snapshot_id,
    source_environment,
    source_host,
    source_database,
    dump_file_name,
    dump_sha256,
    imported_by,
    note
)
VALUES (
    gen_random_uuid(),
    'development',
    'fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com',
    'FPS',
    'fps_dev_snapshot.dump',
    NULL,
    current_user,
    'Initial sink import'
);
```

### C) Run curated extraction and foundation sync

```bash
psql "$SINK_CONN" -v ON_ERROR_STOP=1 -f database/sink/sql/020_curated_from_raw_template.sql
psql "$SINK_CONN" -v ON_ERROR_STOP=1 -f database/sink/sql/030_sync_foundation_from_curated_template.sql
```

## Guardrails

- Do not run transformations directly against cloud DB.
- Keep curated tables minimal and purpose-specific.
- Keep all sync scripts idempotent.
- Avoid storing sensitive secrets in scripts or source control.
- Record each run in `sink_meta.pipeline_run` for traceability.
