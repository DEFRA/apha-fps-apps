-- Migration 012: Remove deprecated operational schema.
-- Safe to re-run.

BEGIN;

DROP SCHEMA IF EXISTS operational CASCADE;

COMMIT;
