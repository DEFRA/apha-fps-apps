#!/bin/bash

set -euo pipefail

DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-batch_jobs_foundation_db}
DB_USER=${DB_USER:-postgres}

ROOT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)

run_sql_file() {
    local file_path="$1"
    echo "[INFO] Applying ${file_path#$ROOT_DIR/}"
    psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -v ON_ERROR_STOP=1 \
        -f "$file_path"
}

echo "[INFO] Resetting ScheduledLoadFromFps local database state"

run_sql_file "$ROOT_DIR/database/sql/flush/002_flush_scheduled_load_tables.sql"
run_sql_file "$ROOT_DIR/database/sql/seeds/001_seed_scheduled_job_master.sql"
run_sql_file "$ROOT_DIR/database/sql/seeds/002_seed_scheduled_source_baseline.sql"
run_sql_file "$ROOT_DIR/database/sql/seeds/003_seed_scheduled_validation_baseline.sql"

echo "[INFO] Final row count summary"
psql \
    -h "$DB_HOST" \
    -p "$DB_PORT" \
    -U "$DB_USER" \
    -d "$DB_NAME" \
    -v ON_ERROR_STOP=1 \
    -c "SELECT
          (SELECT COUNT(*) FROM fps.scheduled_load_run) AS scheduled_load_run_count,
          (SELECT COUNT(*) FROM fps.scheduled_load_step_run) AS scheduled_load_step_run_count,
          (SELECT COUNT(*) FROM fps.scheduled_load_validation_result) AS scheduled_load_validation_result_count,
          (SELECT COUNT(*) FROM fps.fpsyeartotals) AS fpsyeartotals_count,
          (SELECT COUNT(*) FROM fps.tlkpproject) AS tlkpproject_count,
          (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals) AS my_fpsyeartotals_count,
          (SELECT COUNT(*) FROM mabarchive.my_tlkpproject_all) AS my_tlkpproject_all_count;"

echo "[INFO] Reset complete"