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

run_sql_file "$ROOT_DIR/database/sql/flush/001_flush_operational_schema.sql"

for file in "$ROOT_DIR"/database/sql/*.sql; do
    if [[ -f "$file" ]]; then
        run_sql_file "$file"
    fi
done

for file in "$ROOT_DIR"/database/sql/seeds/*.sql; do
    if [[ -f "$file" ]]; then
        run_sql_file "$file"
    fi
done

echo "[INFO] Reset complete"