-- Migration 011: Remove redundant ScheduledLoadFromFps intermediate tables.
-- These tables are intentionally retired for now.
-- Safe to re-run.

BEGIN;

-- Current runtime schema after migration normalisation.
DROP TABLE IF EXISTS fps.fps_project_all_current_year;
DROP TABLE IF EXISTS fps.fps_source_project_year;
DROP TABLE IF EXISTS fps.fps_year_archive;
DROP TABLE IF EXISTS fps.fps_year_totals;

-- Safety for environments where pre-normalisation schema names still exist.
DROP TABLE IF EXISTS operational.fps_project_all_current_year;
DROP TABLE IF EXISTS operational.fps_source_project_year;
DROP TABLE IF EXISTS operational.fps_year_archive;
DROP TABLE IF EXISTS operational.fps_year_totals;

COMMIT;
