--liquibase formatted sql

--changeset arihant:CR099 labels:ddl context:all splitStatements:false

/* CR: Drop unused columns from fps.job_queue
   Columns: failure_reason, heartbeat_at_utc, upload_checksum_sha256, s3_object_key
   None are read or written by the application. Worker heartbeat is held on fps.job_lock.expires_at.
   Run when no batch jobs are executing (ALTER TABLE takes a brief ACCESS EXCLUSIVE lock). */

BEGIN;

/* Pre-check: stop if any of the columns hold data */
DO $$
DECLARE
    v_rows bigint;
BEGIN
    SELECT count(*) INTO v_rows
    FROM fps.job_queue
    WHERE failure_reason IS NOT NULL
       OR heartbeat_at_utc IS NOT NULL
       OR upload_checksum_sha256 IS NOT NULL
       OR s3_object_key IS NOT NULL;

    IF v_rows > 0 THEN
        RAISE EXCEPTION 'Aborting: % job_queue row(s) have data in the columns being dropped', v_rows;
    END IF;
END $$;

DROP INDEX IF EXISTS fps.idx_job_queue_statusid_heartbeat_at_utc;

ALTER TABLE fps.job_queue
    DROP COLUMN IF EXISTS failure_reason,
    DROP COLUMN IF EXISTS heartbeat_at_utc,
    DROP COLUMN IF EXISTS upload_checksum_sha256,
    DROP COLUMN IF EXISTS s3_object_key;

/* Post-check: expect 0 rows */
SELECT column_name
FROM information_schema.columns
WHERE table_schema = 'fps'
  AND table_name   = 'job_queue'
  AND column_name IN ('failure_reason', 'heartbeat_at_utc', 'upload_checksum_sha256', 's3_object_key');

COMMIT;