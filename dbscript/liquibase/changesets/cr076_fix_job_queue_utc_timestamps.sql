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
--   - approved_at_utc, rejected_at_utc, and triggered_at_utc may each be
--     timestamp without time zone or timestamp with time zone.
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

    -- All three columns may be either the legacy type or the corrected type.
    -- This allows CR076 to recover from a partially applied/manual migration.
    SELECT COUNT(*)
      INTO v_count
      FROM information_schema.columns
     WHERE table_schema = 'fps'
       AND table_name = 'job_queue'
       AND column_name IN (
           'approved_at_utc',
           'rejected_at_utc',
           'triggered_at_utc'
       )
       AND data_type IN (
           'timestamp without time zone',
           'timestamp with time zone'
       );

    IF v_count <> 3 THEN
        RAISE EXCEPTION
            'CR076 precondition failed: lifecycle timestamp columns must be timestamp with or without time zone.';
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

DO $$
DECLARE
    v_column record;
BEGIN
    FOR v_column IN
        SELECT column_name
          FROM information_schema.columns
         WHERE table_schema = 'fps'
           AND table_name = 'job_queue'
           AND column_name IN (
               'approved_at_utc',
               'rejected_at_utc',
               'triggered_at_utc'
           )
           AND data_type = 'timestamp without time zone'
    LOOP
        EXECUTE format(
            'ALTER TABLE fps.job_queue ALTER COLUMN %I TYPE timestamp with time zone USING %I AT TIME ZONE ''UTC''',
            v_column.column_name,
            v_column.column_name
        );
    END LOOP;
END
$$;

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
