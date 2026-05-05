-- Rollback script for operational -> fps migration
-- Use if migration needs to be reversed
-- This script removes fps tables and views, keeping operational legacy tables intact

BEGIN;

-- Drop backward-compatibility views first (requires CASCADE for dependent views)
DROP VIEW IF EXISTS operational.batch_lock CASCADE;
DROP VIEW IF EXISTS operational.tbljobmaster CASCADE;
DROP VIEW IF EXISTS operational.tbljobstatus CASCADE;
DROP VIEW IF EXISTS operational.tbljobqueue CASCADE;
DROP VIEW IF EXISTS operational.tbljobqueue_log CASCADE;

-- Drop fps tables with CASCADE for dependent tables
DROP TABLE IF EXISTS fps.job_queue_log CASCADE;
DROP TABLE IF EXISTS fps.job_queue CASCADE;
DROP TABLE IF EXISTS fps.job_status CASCADE;
DROP TABLE IF EXISTS fps.job_master CASCADE;
DROP TABLE IF EXISTS fps.job_lock CASCADE;

-- Drop fps schema if empty
DROP SCHEMA IF EXISTS fps CASCADE;

RAISE NOTICE 'Rollback complete. fps schema and all views dropped. operational legacy tables remain intact.';

COMMIT;
