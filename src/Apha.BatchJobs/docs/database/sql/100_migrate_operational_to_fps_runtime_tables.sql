-- Migration 100 (deprecated)
-- Legacy bridge migration retired.
-- Current runtime model is fps-only.
-- Use 001, 003, 004, 011, and 012 scripts for active schema management.

BEGIN;

-- No-op retained for sequence/history compatibility.
SELECT 1;

COMMIT;
