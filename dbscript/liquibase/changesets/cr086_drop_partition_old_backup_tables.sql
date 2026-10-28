--liquibase formatted sql

--changeset repo-admin:CR086 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR086
--
-- Purpose:
--   Drop the *_old backup tables retained by CR071, CR077, CR082, CR083 and
--   CR084 now that partitioning migration and deployment verification have
--   completed successfully:
--
--     1. fps.rate_change_history_old        (CR071)
--     2. fps.notification_run_summary_old   (CR071 / CR074)
--     3. fps.job_queue_log_old              (CR077)
--     4. fps.period_timecostcalcs_old       (CR082)
--     5. fps.period_proj_subcontract_old    (CR083)
--     6. fps.period_monthlyoutput_old       (CR084)
--
-- Only run this CR after CR071, CR074, CR077, CR082, CR083 and CR084 have
-- all been verified in this environment. This is irreversible.
-- ============================================================================

DROP TABLE IF EXISTS fps.rate_change_history_old;
DROP TABLE IF EXISTS fps.notification_run_summary_old;
DROP TABLE IF EXISTS fps.job_queue_log_old;
DROP TABLE IF EXISTS fps.period_timecostcalcs_old;
DROP TABLE IF EXISTS fps.period_proj_subcontract_old;
DROP TABLE IF EXISTS fps.period_monthlyoutput_old;

COMMIT;
