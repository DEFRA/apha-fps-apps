--liquibase formatted sql

--changeset repo-admin:CR071 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR071
--
-- Purpose:
--   Partition durable FPS / BatchJobs audit tables by FPS year:
--
--     1. fps.rate_change_history
--     2. fps.notification_run_summary
--
-- Design:
--   - PARTITION BY LIST (fpsyear)
--   - fps.job_queue remains UNPARTITIONED.
--   - FKs to fps.job_queue therefore remain on jobqueueid only.
--   - Existing data is preserved.
--   - Explicit partitions: 2016..2027 + DEFAULT.
--   - Original tables are retained as *_old after successful migration.
--
-- CR074 depends on this CR having completed successfully.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

    IF to_regclass('fps.rate_change_history') IS NULL THEN
        RAISE EXCEPTION
            'CR071 precondition failed: fps.rate_change_history does not exist.';
    END IF;


    IF to_regclass('fps.notification_run_summary') IS NULL THEN
        RAISE EXCEPTION
            'CR071 precondition failed: fps.notification_run_summary does not exist.';
    END IF;


    -- Protect against accidental rerun or a partially completed migration.
    IF to_regclass('fps.rate_change_history_p') IS NOT NULL
       OR to_regclass('fps.rate_change_history_old') IS NOT NULL THEN

        RAISE EXCEPTION
            'CR071 precondition failed: rate_change_history migration/backup table already exists.';

    END IF;


    IF to_regclass('fps.notification_run_summary_p') IS NOT NULL
       OR to_regclass('fps.notification_run_summary_old') IS NOT NULL THEN

        RAISE EXCEPTION
            'CR071 precondition failed: notification_run_summary migration/backup table already exists.';

    END IF;


    -- rate_change_history.fpsyear was originally introduced nullable.
    -- Do not guess historical years.
    IF EXISTS (
        SELECT 1
        FROM fps.rate_change_history
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR071 blocked: fps.rate_change_history contains NULL fpsyear rows. Apply the approved deterministic backfill before running CR071.';

    END IF;


    -- notification_run_summary should already be NOT NULL, but validate
    -- the actual database state before partition conversion.
    IF EXISTS (
        SELECT 1
        FROM fps.notification_run_summary
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR071 blocked: fps.notification_run_summary contains NULL fpsyear rows.';

    END IF;


    -- A table-swap migration would leave any unknown incoming FK pointing
    -- at the renamed *_old table. Fail closed if such a dependency exists.
    IF EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE contype = 'f'
          AND confrelid = 'fps.rate_change_history'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR071 precondition failed: another table has an FK to fps.rate_change_history. Review dependency before partition conversion.';

    END IF;


    IF EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE contype = 'f'
          AND confrelid = 'fps.notification_run_summary'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR071 precondition failed: another table has an FK to fps.notification_run_summary. Review dependency before partition conversion.';

    END IF;

END
$$;


-- Prevent writes during copy and table swap.
LOCK TABLE
    fps.rate_change_history,
    fps.notification_run_summary
IN ACCESS EXCLUSIVE MODE;


-- Partition keys become part of the primary keys.
ALTER TABLE fps.rate_change_history
    ALTER COLUMN fpsyear SET NOT NULL;


ALTER TABLE fps.notification_run_summary
    ALTER COLUMN fpsyear SET NOT NULL;


-- ============================================================================
-- 1. fps.rate_change_history
-- ============================================================================

CREATE TABLE fps.rate_change_history_p
(
    LIKE fps.rate_change_history
    INCLUDING ALL
    EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


-- Partitioned-table primary key must include the partition key.
ALTER TABLE fps.rate_change_history_p

    ADD CONSTRAINT rate_change_history_p_pkey
        PRIMARY KEY (
            ratechangehistoryid,
            fpsyear
        ),

    ADD CONSTRAINT fk_rate_change_history_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid),

    ADD CONSTRAINT fk_rate_change_history_job_master
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master (jobid);


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
    y integer;
BEGIN

    FOR y IN 2016..2027 LOOP

        EXECUTE format(
            'CREATE TABLE fps.rate_change_history_p_y%s
             PARTITION OF fps.rate_change_history_p
             FOR VALUES IN (%s)',
            y,
            y
        );

    END LOOP;

END
$$;


-- Safety partition for any FPS year not explicitly provisioned.
CREATE TABLE fps.rate_change_history_p_default
    PARTITION OF fps.rate_change_history_p
    DEFAULT;


-- Preserve existing data.
INSERT INTO fps.rate_change_history_p
SELECT *
FROM fps.rate_change_history;


-- Verify row-count parity before table swap.
DO $$
DECLARE
    v_source_count bigint;
    v_target_count bigint;
BEGIN

    SELECT COUNT(*)
    INTO v_source_count
    FROM fps.rate_change_history;


    SELECT COUNT(*)
    INTO v_target_count
    FROM fps.rate_change_history_p;


    IF v_source_count <> v_target_count THEN

        RAISE EXCEPTION
            'CR071 rate_change_history row-count validation failed: source %, target %.',
            v_source_count,
            v_target_count;

    END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.rate_change_history
    RENAME TO rate_change_history_old;


ALTER TABLE fps.rate_change_history_p
    RENAME TO rate_change_history;


-- ============================================================================
-- 1a. rate_change_history indexes
-- ============================================================================

-- The renamed backup table retains its existing index names.
-- Rename them so the production names can be reused.

ALTER INDEX IF EXISTS fps.idx_rate_change_history_jobqueueid
    RENAME TO idx_rate_change_history_old_jobqueueid;


ALTER INDEX IF EXISTS fps.idx_rate_change_history_businesskey
    RENAME TO idx_rate_change_history_old_businesskey;


ALTER INDEX IF EXISTS fps.idx_rate_change_history_jobid
    RENAME TO idx_rate_change_history_old_jobid;


ALTER INDEX IF EXISTS fps.idx_rate_change_history_fpsyear
    RENAME TO idx_rate_change_history_old_fpsyear;


-- Recreate required indexes on the partitioned production table.

CREATE INDEX idx_rate_change_history_jobqueueid
    ON fps.rate_change_history (jobqueueid);


CREATE INDEX idx_rate_change_history_businesskey
    ON fps.rate_change_history (
        ratecategory,
        businesskey
    );


CREATE INDEX idx_rate_change_history_jobid
    ON fps.rate_change_history (jobid);


CREATE INDEX idx_rate_change_history_fpsyear
    ON fps.rate_change_history (fpsyear);


-- ============================================================================
-- 1b. rate_change_history sequence ownership
-- ============================================================================

-- The replacement table copied the bigserial DEFAULT from the source table.
-- Detach the backup from that sequence and make the production table its owner.

ALTER TABLE fps.rate_change_history_old
    ALTER COLUMN ratechangehistoryid DROP DEFAULT;


DO $$
DECLARE
    v_max_id bigint;
BEGIN

    IF to_regclass(
        'fps.rate_change_history_ratechangehistoryid_seq'
    ) IS NOT NULL THEN

        ALTER SEQUENCE
            fps.rate_change_history_ratechangehistoryid_seq
        OWNED BY
            fps.rate_change_history.ratechangehistoryid;


        SELECT COALESCE(MAX(ratechangehistoryid), 0)
        INTO v_max_id
        FROM fps.rate_change_history;


        PERFORM setval(
            'fps.rate_change_history_ratechangehistoryid_seq'::regclass,
            GREATEST(v_max_id, 1),
            v_max_id > 0
        );

    END IF;

END
$$;


-- ============================================================================
-- 2. fps.notification_run_summary
-- ============================================================================

CREATE TABLE fps.notification_run_summary_p
(
    LIKE fps.notification_run_summary
    INCLUDING ALL
    EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


ALTER TABLE fps.notification_run_summary_p

    ADD CONSTRAINT notification_run_summary_p_pkey
        PRIMARY KEY (
            notificationrunsummaryid,
            fpsyear
        ),

    ADD CONSTRAINT fk_notification_run_summary_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid),

    -- Application uses:
    --
    --     ON CONFLICT (jobqueueid, fpsyear)
    --
    -- The partition key must therefore be included in the unique constraint.
    ADD CONSTRAINT uq_notification_run_summary_jobqueueid_fpsyear
        UNIQUE (
            jobqueueid,
            fpsyear
        );


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
    y integer;
BEGIN

    FOR y IN 2016..2027 LOOP

        EXECUTE format(
            'CREATE TABLE fps.notification_run_summary_p_y%s
             PARTITION OF fps.notification_run_summary_p
             FOR VALUES IN (%s)',
            y,
            y
        );

    END LOOP;

END
$$;


CREATE TABLE fps.notification_run_summary_p_default
    PARTITION OF fps.notification_run_summary_p
    DEFAULT;


-- Preserve existing data.
INSERT INTO fps.notification_run_summary_p
SELECT *
FROM fps.notification_run_summary;


-- Verify row-count parity before table swap.
DO $$
DECLARE
    v_source_count bigint;
    v_target_count bigint;
BEGIN

    SELECT COUNT(*)
    INTO v_source_count
    FROM fps.notification_run_summary;


    SELECT COUNT(*)
    INTO v_target_count
    FROM fps.notification_run_summary_p;


    IF v_source_count <> v_target_count THEN

        RAISE EXCEPTION
            'CR071 notification_run_summary row-count validation failed: source %, target %.',
            v_source_count,
            v_target_count;

    END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.notification_run_summary
    RENAME TO notification_run_summary_old;


ALTER TABLE fps.notification_run_summary_p
    RENAME TO notification_run_summary;


-- ============================================================================
-- 2a. notification_run_summary legacy index
-- ============================================================================

-- The old table retains the original unique-index name.
-- Rename it to make the backup clearly identifiable.

ALTER INDEX IF EXISTS fps.ux_notification_run_summary_jobqueue
    RENAME TO ux_notification_run_summary_old_jobqueue;


-- ============================================================================
-- 2b. notification_run_summary sequence ownership
-- ============================================================================

-- CR074 subsequently converts notificationrunsummaryid from bigint to UUID.
-- Until CR074 executes, the existing bigint sequence must belong to the new
-- partitioned production table.

ALTER TABLE fps.notification_run_summary_old
    ALTER COLUMN notificationrunsummaryid DROP DEFAULT;


DO $$
DECLARE
    v_max_id bigint;
BEGIN

    IF to_regclass(
        'fps.notification_run_summary_notificationrunsummaryid_seq'
    ) IS NOT NULL THEN

        ALTER SEQUENCE
            fps.notification_run_summary_notificationrunsummaryid_seq
        OWNED BY
            fps.notification_run_summary.notificationrunsummaryid;


        SELECT COALESCE(MAX(notificationrunsummaryid), 0)
        INTO v_max_id
        FROM fps.notification_run_summary;


        PERFORM setval(
            'fps.notification_run_summary_notificationrunsummaryid_seq'::regclass,
            GREATEST(v_max_id, 1),
            v_max_id > 0
        );

    END IF;

END
$$;


-- ============================================================================
-- 3. Postconditions
-- ============================================================================

DO $$
DECLARE
    v_rate_partition_count integer;
    v_summary_partition_count integer;
BEGIN

    -- Both production tables must now be partitioned.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.rate_change_history'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: fps.rate_change_history is not partitioned.';

    END IF;


    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.notification_run_summary'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: fps.notification_run_summary is not partitioned.';

    END IF;


    -- Partition keys must be complete.
    IF EXISTS (
        SELECT 1
        FROM fps.rate_change_history
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: rate_change_history contains NULL fpsyear.';

    END IF;


    IF EXISTS (
        SELECT 1
        FROM fps.notification_run_summary
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: notification_run_summary contains NULL fpsyear.';

    END IF;


    -- Validate partition count:
    -- 12 yearly partitions (2016..2027) + DEFAULT = 13.

    SELECT COUNT(*)
    INTO v_rate_partition_count
    FROM pg_inherits
    WHERE inhparent =
          'fps.rate_change_history'::regclass;


    IF v_rate_partition_count <> 13 THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: expected 13 rate_change_history partitions, found %.',
            v_rate_partition_count;

    END IF;


    SELECT COUNT(*)
    INTO v_summary_partition_count
    FROM pg_inherits
    WHERE inhparent =
          'fps.notification_run_summary'::regclass;


    IF v_summary_partition_count <> 13 THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: expected 13 notification_run_summary partitions, found %.',
            v_summary_partition_count;

    END IF;


    -- Application ON CONFLICT target must exist.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid =
              'fps.notification_run_summary'::regclass
          AND conname =
              'uq_notification_run_summary_jobqueueid_fpsyear'
    ) THEN

        RAISE EXCEPTION
            'CR071 postcondition failed: notification_run_summary composite unique constraint is missing.';

    END IF;

END
$$;


COMMIT;


--rollback LOCK TABLE fps.rate_change_history, fps.rate_change_history_old, fps.notification_run_summary, fps.notification_run_summary_old IN ACCESS EXCLUSIVE MODE;
--rollback ALTER TABLE fps.rate_change_history RENAME TO rate_change_history_partitioned;
--rollback ALTER TABLE fps.notification_run_summary RENAME TO notification_run_summary_partitioned;
--rollback ALTER TABLE fps.rate_change_history_old RENAME TO rate_change_history;
--rollback ALTER TABLE fps.notification_run_summary_old RENAME TO notification_run_summary;
--rollback ALTER SEQUENCE fps.rate_change_history_ratechangehistoryid_seq OWNED BY fps.rate_change_history.ratechangehistoryid;
--rollback ALTER TABLE fps.rate_change_history ALTER COLUMN ratechangehistoryid SET DEFAULT nextval('fps.rate_change_history_ratechangehistoryid_seq'::regclass);
--rollback ALTER SEQUENCE fps.notification_run_summary_notificationrunsummaryid_seq OWNED BY fps.notification_run_summary.notificationrunsummaryid;
--rollback ALTER TABLE fps.notification_run_summary ALTER COLUMN notificationrunsummaryid SET DEFAULT nextval('fps.notification_run_summary_notificationrunsummaryid_seq'::regclass);
--rollback DROP TABLE fps.rate_change_history_partitioned CASCADE;
--rollback DROP TABLE fps.notification_run_summary_partitioned CASCADE;
--rollback ALTER INDEX fps.idx_rate_change_history_old_jobqueueid RENAME TO idx_rate_change_history_jobqueueid;
--rollback ALTER INDEX fps.idx_rate_change_history_old_businesskey RENAME TO idx_rate_change_history_businesskey;
--rollback ALTER INDEX fps.idx_rate_change_history_old_jobid RENAME TO idx_rate_change_history_jobid;
--rollback ALTER INDEX fps.idx_rate_change_history_old_fpsyear RENAME TO idx_rate_change_history_fpsyear;
--rollback ALTER INDEX fps.ux_notification_run_summary_old_jobqueue RENAME TO ux_notification_run_summary_jobqueue;


-- ============================================================================
-- IMPORTANT
-- ============================================================================
--
-- Do NOT drop:
--
--     fps.rate_change_history_old
--     fps.notification_run_summary_old
--
-- as part of CR071.
--
-- Retain both backup tables until CR071 / CR074 deployment verification
-- has completed successfully.
--
-- CR074 must execute after CR071.
--
-- Future FPS-year partitions are an infrastructure / DBA responsibility.
-- The Worker remains partition-agnostic.
-- ============================================================================
