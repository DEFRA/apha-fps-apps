-- Migration 102 (updated)
-- Validate fps runtime tables required by BatchJobs.

BEGIN;

SELECT 'fps.job_master' AS object_name, COUNT(*)::bigint AS row_count FROM fps.job_master
UNION ALL
SELECT 'fps.job_status', COUNT(*)::bigint FROM fps.job_status
UNION ALL
SELECT 'fps.job_queue', COUNT(*)::bigint FROM fps.job_queue
UNION ALL
SELECT 'fps.job_queue_log', COUNT(*)::bigint FROM fps.job_queue_log
UNION ALL
SELECT 'fps.job_lock', COUNT(*)::bigint FROM fps.job_lock;

COMMIT;
