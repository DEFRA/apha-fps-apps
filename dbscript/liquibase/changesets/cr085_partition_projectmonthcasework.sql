--liquibase formatted sql

--changeset Arihant:CR085 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR085
--
-- Purpose:
--   Partition the FPS year-aware RecreateSummary casework table:
--
--     fps.projectmonthcasework
--
-- Design:
--   - Requires the fpsyear column and deterministic backfill from CR043.
--   - Convert to PARTITION BY LIST (fpsyear).
--   - Partition key remains part of the primary key.
--   - Existing data is preserved.
--   - Explicit partitions: 2016..2027 + DEFAULT.
--   - Original table is used temporarily as *_old during migration and
--     dropped after all validation succeeds.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

    IF to_regclass('fps.projectmonthcasework') IS NULL THEN
        RAISE EXCEPTION
            'CR085 precondition failed: fps.projectmonthcasework does not exist.';
    END IF;

    IF to_regclass('fps.projectmonthcasework_p') IS NOT NULL
       OR to_regclass('fps.projectmonthcasework_old') IS NOT NULL THEN

        RAISE EXCEPTION
            'CR085 precondition failed: projectmonthcasework migration/backup table already exists.';

    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'projectmonthcasework'
          AND column_name = 'fpsyear'
    ) THEN

        RAISE EXCEPTION
            'CR085 precondition failed: fps.projectmonthcasework.fpsyear does not exist. Run CR043 first.';

    END IF;

    IF EXISTS (
        SELECT 1
        FROM fps.projectmonthcasework
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR085 precondition failed: projectmonthcasework contains NULL fpsyear rows.';

    END IF;

    IF EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE contype = 'f'
          AND confrelid = 'fps.projectmonthcasework'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR085 precondition failed: another table has an FK to fps.projectmonthcasework. Review dependency before partition conversion.';

    END IF;

END
$$;


-- Prevent writes during copy and table swap.
LOCK TABLE
    fps.projectmonthcasework
IN ACCESS EXCLUSIVE MODE;


-- ============================================================================
-- 1. Build partitioned replacement table
-- ============================================================================

CREATE TABLE fps.projectmonthcasework_p
(
    LIKE fps.projectmonthcasework
    INCLUDING ALL
    EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


ALTER TABLE fps.projectmonthcasework_p
    ADD CONSTRAINT projectmonthcasework_p_pk
        PRIMARY KEY (
            project,
            monthno,
            fpsyear
        );


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
    y integer;
BEGIN

    FOR y IN 2016..2027 LOOP

        EXECUTE format(
            'CREATE TABLE fps.projectmonthcasework_p_y%s
             PARTITION OF fps.projectmonthcasework_p
             FOR VALUES IN (%s)',
            y,
            y
        );

    END LOOP;

END
$$;


CREATE TABLE fps.projectmonthcasework_p_default
    PARTITION OF fps.projectmonthcasework_p
    DEFAULT;


-- Preserve existing data.
INSERT INTO fps.projectmonthcasework_p
SELECT *
FROM fps.projectmonthcasework;


-- Verify row-count parity before table swap.
DO $$
DECLARE
    v_source_count bigint;
    v_target_count bigint;
BEGIN

    SELECT COUNT(*)
    INTO v_source_count
    FROM fps.projectmonthcasework;

    SELECT COUNT(*)
    INTO v_target_count
    FROM fps.projectmonthcasework_p;

    IF v_source_count <> v_target_count THEN

        RAISE EXCEPTION
            'CR085 projectmonthcasework row-count validation failed: source %, target %.',
            v_source_count,
            v_target_count;

    END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.projectmonthcasework
    RENAME TO projectmonthcasework_old;


ALTER TABLE fps.projectmonthcasework_p
    RENAME TO projectmonthcasework;


-- ============================================================================
-- 2. Indexes
-- ============================================================================

CREATE INDEX idx_projectmonthcasework_project
    ON fps.projectmonthcasework (project);


CREATE INDEX idx_projectmonthcasework_monthno
    ON fps.projectmonthcasework (monthno);


CREATE INDEX idx_projectmonthcasework_fpsyear
    ON fps.projectmonthcasework (fpsyear);


-- ============================================================================
-- 3. Postconditions
-- ============================================================================

DO $$
DECLARE
    v_partition_count integer;
BEGIN

    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.projectmonthcasework'::regclass
    ) THEN

        RAISE EXCEPTION
            'CR085 postcondition failed: fps.projectmonthcasework is not partitioned.';

    END IF;

    IF EXISTS (
        SELECT 1
        FROM fps.projectmonthcasework
        WHERE fpsyear IS NULL
    ) THEN

        RAISE EXCEPTION
            'CR085 postcondition failed: projectmonthcasework contains NULL fpsyear.';

    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'fps.projectmonthcasework'::regclass
          AND contype = 'p'
          AND conname = 'projectmonthcasework_p_pk'
    ) THEN

        RAISE EXCEPTION
            'CR085 postcondition failed: projectmonthcasework primary key is missing.';

    END IF;

    -- 12 yearly partitions (2016..2027) + DEFAULT = 13.
    SELECT COUNT(*)
    INTO v_partition_count
    FROM pg_inherits
    WHERE inhparent =
          'fps.projectmonthcasework'::regclass;

    IF v_partition_count <> 13 THEN

        RAISE EXCEPTION
            'CR085 postcondition failed: expected 13 projectmonthcasework partitions, found %.',
            v_partition_count;

    END IF;

END
$$;


DROP TABLE fps.projectmonthcasework_old;


COMMIT;


-- ============================================================================
-- Future FPS-year partitions are an infrastructure / DBA responsibility.
-- ============================================================================
