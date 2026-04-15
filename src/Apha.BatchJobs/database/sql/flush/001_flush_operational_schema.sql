-- Flush script for local/dev reset cycles.
-- WARNING: This removes all objects and data in schema operational.

BEGIN;

DROP SCHEMA IF EXISTS operational CASCADE;
CREATE SCHEMA operational;

COMMIT;
