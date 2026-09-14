--liquibase formatted sql

--changeset repo-admin:CR076 labels:ddl context:all splitStatements:false
--validCheckSum: ANY

BEGIN;

-- ============================================================
-- CR076
-- Purpose:
--   Permanently correct fps.job_queue UTC lifecycle timestamp
--   columns from timestamp without time zone to timestamptz.
--
-- Scope:
--   batchjobs only
--
-- Expected schema before execution:
--   - approved_at_utc  = timestamp without time zone
--   - rejected_at_utc  = timestamp without time zone
--   - triggered_at_utc = timestamp without time zone
--   - cancelled_at_utc = timestamp with time zone
--
-- Existing values in the three columns are UTC wall-clock timestamps and
-- are converted explicitly with AT TIME ZONE 'UTC'.
-- ============================================================

DO $$
DECLARE
    v_count integer;
BEGIN
    -- Table must exist.
    IF to_regclass('fps.job_queue') IS NULL THEN
        RAISE EXCEPTION 'CR076 precondition failed: fps.job_queue does not exist.';
    END IF;

    -- approved_at_utc must still be timestamp without time zone.
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name = 'approved_at_utc'
       AND data_type = 'timestamp without time zone';

    IF v_count <> 1 THEN
        RAISE EXCEPTION
            'CR076 precondition failed: fps.job_queue.approved_at_utc is not timestamp without time zone.';
    END IF;

    -- rejected_at_utc must still be timestamp without time zone.
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name = 'rejected_at_utc'
       AND data_type = 'timestamp without time zone';

    IF v_count <> 1 THEN
        RAISE EXCEPTION
            'CR076 precondition failed: fps.job_queue.rejected_at_utc is not timestamp without time zone.';
    END IF;

    -- triggered_at_utc must still be timestamp without time zone.
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name = 'triggered_at_utc'
       AND data_type = 'timestamp without time zone';

    IF v_count <> 1 THEN
        RAISE EXCEPTION
            'CR076 precondition failed: fps.job_queue.triggered_at_utc is not timestamp without time zone.';
    END IF;

    -- cancelled_at_utc is deliberately NOT changed by this CR.
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name = 'cancelled_at_utc'
       AND data_type = 'timestamp with time zone';

    IF v_count <> 1 THEN
        RAISE EXCEPTION
            'CR076 precondition failed: fps.job_queue.cancelled_at_utc is not timestamp with time zone.';
    END IF;

END
$$;

ALTER TABLE fps.job_queue
    ALTER COLUMN approved_at_utc
        TYPE timestamp with time zone
        USING approved_at_utc AT TIME ZONE 'UTC',

    ALTER COLUMN rejected_at_utc
        TYPE timestamp with time zone
        USING rejected_at_utc AT TIME ZONE 'UTC',

    ALTER COLUMN triggered_at_utc
        TYPE timestamp with time zone
        USING triggered_at_utc AT TIME ZONE 'UTC';

DO $$
DECLARE
    v_count integer;
BEGIN
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name IN (
           'approved_at_utc',
           'rejected_at_utc',
           'triggered_at_utc',
           'cancelled_at_utc'
       )
       AND data_type = 'timestamp with time zone';

    IF v_count <> 4 THEN
        RAISE EXCEPTION
            'CR076 postcondition failed: expected all 4 lifecycle timestamp columns to be timestamp with time zone; found %.',
            v_count;
    END IF;
END
$$;

COMMIT;
