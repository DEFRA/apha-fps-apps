--liquibase formatted sql

--changeset Nandkishor:CR084 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR084
--
-- Purpose:
--   Introduce FPS-year scoping for the Department Income snapshot table:
--
--     fps.period_monthlyoutput
--
--   This table backs the "Tests" query option on the Snapshot Data tab
--   (SQL Server fPeriodTests equivalent) and contributes to "Totals".
--   SQL Server stores one snapshot table per FPS year; PostgreSQL keeps
--   multiple years in a single table, so an fpsyear discriminator is
--   required to restore parity.
--
-- Design:
--   - Add fpsyear (nullable first; deterministic backfill required).
--   - Convert to PARTITION BY LIST (fpsyear).
--   - Partition key becomes part of the primary key.
--   - Existing data is preserved.
--   - Explicit partitions: 2016..2027 + DEFAULT.
--   - Original table is retained as *_old after successful migration.
--
-- NOTE:
--   fps.period_monthlyoutput did NOT previously have an fpsyear column.
--   A deterministic backfill MUST be applied before this CR is run, OR the
--   backfill literal below must be set to the correct single year this
--   snapshot data belongs to. Do NOT guess historical years.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

	IF to_regclass('fps.period_monthlyoutput') IS NULL THEN
		RAISE EXCEPTION
			'CR084 precondition failed: fps.period_monthlyoutput does not exist.';
	END IF;

	IF to_regclass('fps.period_monthlyoutput_p') IS NOT NULL
	   OR to_regclass('fps.period_monthlyoutput_old') IS NOT NULL THEN

		RAISE EXCEPTION
			'CR084 precondition failed: period_monthlyoutput migration/backup table already exists.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM pg_constraint
		WHERE contype = 'f'
		  AND confrelid = 'fps.period_monthlyoutput'::regclass
	) THEN

		RAISE EXCEPTION
			'CR084 precondition failed: another table has an FK to fps.period_monthlyoutput. Review dependency before partition conversion.';

	END IF;

END
$$;


-- Prevent writes during copy and table swap.
LOCK TABLE
	fps.period_monthlyoutput
IN ACCESS EXCLUSIVE MODE;


-- ============================================================================
-- 1. Add and backfill fpsyear
-- ============================================================================

ALTER TABLE fps.period_monthlyoutput
	ADD COLUMN IF NOT EXISTS fpsyear integer;


-- Deterministic backfill.
-- Replace the literal below with the approved value (or an approved
-- multi-year backfill statement) before running CR084 in each environment.
UPDATE fps.period_monthlyoutput
SET fpsyear = 2025
WHERE fpsyear IS NULL;


DO $$
BEGIN
	IF EXISTS (
		SELECT 1
		FROM fps.period_monthlyoutput
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR084 blocked: fps.period_monthlyoutput contains NULL fpsyear rows. Apply the approved deterministic backfill before running CR084.';

	END IF;
END
$$;


ALTER TABLE fps.period_monthlyoutput
	ALTER COLUMN fpsyear SET NOT NULL;


-- ============================================================================
-- 2. Build partitioned replacement table
-- ============================================================================

CREATE TABLE fps.period_monthlyoutput_p
(
	LIKE fps.period_monthlyoutput
	INCLUDING ALL
	EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


ALTER TABLE fps.period_monthlyoutput_p
	ADD CONSTRAINT period_monthlyoutput_p_pkey
		PRIMARY KEY (
			id,
			fpsyear
		);


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
	y integer;
BEGIN

	FOR y IN 2016..2027 LOOP

		EXECUTE format(
			'CREATE TABLE fps.period_monthlyoutput_p_y%s
			 PARTITION OF fps.period_monthlyoutput_p
			 FOR VALUES IN (%s)',
			y,
			y
		);

	END LOOP;

END
$$;


CREATE TABLE fps.period_monthlyoutput_p_default
	PARTITION OF fps.period_monthlyoutput_p
	DEFAULT;


-- Preserve existing data.
INSERT INTO fps.period_monthlyoutput_p
SELECT *
FROM fps.period_monthlyoutput;


-- Verify row-count parity before table swap.
DO $$
DECLARE
	v_source_count bigint;
	v_target_count bigint;
BEGIN

	SELECT COUNT(*)
	INTO v_source_count
	FROM fps.period_monthlyoutput;

	SELECT COUNT(*)
	INTO v_target_count
	FROM fps.period_monthlyoutput_p;

	IF v_source_count <> v_target_count THEN

		RAISE EXCEPTION
			'CR084 period_monthlyoutput row-count validation failed: source %, target %.',
			v_source_count,
			v_target_count;

	END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.period_monthlyoutput
	RENAME TO period_monthlyoutput_old;


ALTER TABLE fps.period_monthlyoutput_p
	RENAME TO period_monthlyoutput;


-- ============================================================================
-- 2a. Indexes
-- ============================================================================

CREATE INDEX idx_period_monthlyoutput_project
	ON fps.period_monthlyoutput (project);


CREATE INDEX idx_period_monthlyoutput_period
	ON fps.period_monthlyoutput (period);


CREATE INDEX idx_period_monthlyoutput_fpsyear
	ON fps.period_monthlyoutput (fpsyear);


-- ============================================================================
-- 2b. Sequence ownership
-- ============================================================================

ALTER TABLE fps.period_monthlyoutput_old
	ALTER COLUMN id DROP IDENTITY;


DO $$
DECLARE
	v_max_id bigint;
BEGIN

	IF to_regclass(
		'fps.period_monthlyoutput_id_seq'
	) IS NOT NULL THEN

		ALTER SEQUENCE
			fps.period_monthlyoutput_id_seq
		OWNED BY
			fps.period_monthlyoutput.id;

		SELECT COALESCE(MAX(id), 0)
		INTO v_max_id
		FROM fps.period_monthlyoutput;

		PERFORM setval(
			'fps.period_monthlyoutput_id_seq'::regclass,
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
	v_partition_count integer;
BEGIN

	IF NOT EXISTS (
		SELECT 1
		FROM pg_partitioned_table
		WHERE partrelid =
			  'fps.period_monthlyoutput'::regclass
	) THEN

		RAISE EXCEPTION
			'CR084 postcondition failed: fps.period_monthlyoutput is not partitioned.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM fps.period_monthlyoutput
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR084 postcondition failed: period_monthlyoutput contains NULL fpsyear.';

	END IF;

	-- 12 yearly partitions (2016..2027) + DEFAULT = 13.
	SELECT COUNT(*)
	INTO v_partition_count
	FROM pg_inherits
	WHERE inhparent =
		  'fps.period_monthlyoutput'::regclass;

	IF v_partition_count <> 13 THEN

		RAISE EXCEPTION
			'CR084 postcondition failed: expected 13 period_monthlyoutput partitions, found %.',
			v_partition_count;

	END IF;

END
$$;


COMMIT;


--rollback LOCK TABLE fps.period_monthlyoutput, fps.period_monthlyoutput_old IN ACCESS EXCLUSIVE MODE;
--rollback ALTER TABLE fps.period_monthlyoutput RENAME TO period_monthlyoutput_partitioned;
--rollback ALTER TABLE fps.period_monthlyoutput_old RENAME TO period_monthlyoutput;
--rollback ALTER SEQUENCE fps.period_monthlyoutput_id_seq OWNED BY fps.period_monthlyoutput.id;
--rollback ALTER TABLE fps.period_monthlyoutput ALTER COLUMN id SET DEFAULT nextval('fps.period_monthlyoutput_id_seq'::regclass);
--rollback DROP TABLE fps.period_monthlyoutput_partitioned CASCADE;


-- ============================================================================
-- IMPORTANT
-- ============================================================================
--
-- Do NOT drop:
--
--     fps.period_monthlyoutput_old
--
-- as part of CR084.
--
-- Retain the backup table until deployment verification (Snapshot Tests /
-- Totals grid parity against Access frmDeptIncome.frm) has completed
-- successfully.
--
-- Future FPS-year partitions are an infrastructure / DBA responsibility.
-- ============================================================================
