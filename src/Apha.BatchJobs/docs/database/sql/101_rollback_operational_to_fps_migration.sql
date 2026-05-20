-- Migration 101 (deprecated)
-- Rollback for retired legacy bridge migration.
-- Current runtime model is fps-only.

BEGIN;

-- No-op retained for sequence/history compatibility.
SELECT 1;

COMMIT;
