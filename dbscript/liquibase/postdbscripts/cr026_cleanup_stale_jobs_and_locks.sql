--liquibase formatted sql

--changeset repo-admin:CR026_cleanup labels:dml context:all
--comment: DML extracted from changesets/cr026_modify_ddl_batchjobs.sql. Independent reconciliation cleanup, safe to run after all changesets.

WITH stale_jobs AS (
    SELECT jq.jobqueueid,
           jq.jobid
    FROM fps.job_queue jq
    INNER JOIN fps.job_status js
        ON js.statusid = jq.statusid
    WHERE js.status IN ('Running', 'Initiated')
      AND COALESCE(jq.heartbeat_at_utc, jq.startdatetime)
            < NOW() - INTERVAL '15 minutes'
)
UPDATE fps.job_queue q
SET statusid = failed_status.statusid,
    failure_reason = COALESCE(
        q.failure_reason,
        'Stale in-progress record recovered by can-run reconciliation'
    ),
    enddatetime = COALESCE(q.enddatetime, NOW()),
    updated_at = NOW()
FROM stale_jobs s
INNER JOIN fps.job_status failed_status
    ON failed_status.jobid = s.jobid
   AND failed_status.status = 'Failed'
WHERE q.jobqueueid = s.jobqueueid;

DELETE FROM fps.job_lock
WHERE expires_at < NOW();

--ROLLBACK
--Not Applicable
