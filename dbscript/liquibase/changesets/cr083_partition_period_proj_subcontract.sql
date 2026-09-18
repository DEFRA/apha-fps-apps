--liquibase formatted sql

--changeset Nandkishor:CR083 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR083
--
-- Purpose:
--   Introduce FPS-year scoping for the Department Income snapshot table:
--
--     fps.period_proj_subcontract
--
--   This table backs the "Animals" and "Exceptional" query options on the
--   Snapshot Data tab (SQL Server fPeriodAnimals / fPeriodExceptional
--   equivalents) and contributes to "Totals". SQL Server stores one snapshot
--   table per FPS year; PostgreSQL keeps multiple years in a single table,
--   so an fpsyear discriminator is required to restore parity.
--
-- Design:
--   - Add fpsyear (nullable first; deterministic backfill required).
--   - Convert to PARTITION BY LIST (fpsyear).
--   - This table uses a COMPOSITE natural key (period, subcontcounter).
--     The partition key must be included, so the primary key becomes
--     (period, subcontcounter, fpsyear). There is NO surrogate id column
--     and NO owning sequence.
--   - Existing data is preserved.
--   - Explicit partitions: 2016..2027 + DEFAULT.
--   - Original table is used temporarily as *_old during migration and
--     dropped after all validation succeeds.
--
-- NOTE:
--   fps.period_proj_subcontract did NOT previously have an fpsyear column.
--   A deterministic backfill MUST be applied before this CR is run, OR the
--   backfill literal below must be set to the correct single year this
--   snapshot data belongs to. Do NOT guess historical years.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

	IF to_regclass('fps.period_proj_subcontract') IS NULL THEN
		RAISE EXCEPTION
			'CR083 precondition failed: fps.period_proj_subcontract does not exist.';
	END IF;

	IF to_regclass('fps.period_proj_subcontract_p') IS NOT NULL
	   OR to_regclass('fps.period_proj_subcontract_old') IS NOT NULL THEN

		RAISE EXCEPTION
			'CR083 precondition failed: period_proj_subcontract migration/backup table already exists.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM pg_constraint
		WHERE contype = 'f'
		  AND confrelid = 'fps.period_proj_subcontract'::regclass
	) THEN

		RAISE EXCEPTION
			'CR083 precondition failed: another table has an FK to fps.period_proj_subcontract. Review dependency before partition conversion.';

	END IF;

END
$$;


-- Prevent writes during copy and table swap.
LOCK TABLE
	fps.period_proj_subcontract
IN ACCESS EXCLUSIVE MODE;


-- ============================================================================
-- 1. Add and backfill fpsyear
-- ============================================================================

ALTER TABLE fps.period_proj_subcontract
	ADD COLUMN IF NOT EXISTS fpsyear integer;


-- Deterministic backfill.
-- Replace the literal below with the approved value (or an approved
-- multi-year backfill statement) before running CR083 in each environment.
UPDATE fps.period_proj_subcontract
SET fpsyear = 2025
WHERE fpsyear IS NULL;


DO $$
BEGIN
	IF EXISTS (
		SELECT 1
		FROM fps.period_proj_subcontract
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR083 blocked: fps.period_proj_subcontract contains NULL fpsyear rows. Apply the approved deterministic backfill before running CR083.';

	END IF;
END
$$;


ALTER TABLE fps.period_proj_subcontract
	ALTER COLUMN fpsyear SET NOT NULL;


-- ============================================================================
-- 2. Build partitioned replacement table
-- ============================================================================

CREATE TABLE fps.period_proj_subcontract_p
(
	LIKE fps.period_proj_subcontract
	INCLUDING ALL
	EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


-- Composite natural key; partition key must be part of the primary key.
ALTER TABLE fps.period_proj_subcontract_p
	ADD CONSTRAINT period_proj_subcontract_p_pkey
		PRIMARY KEY (
			period,
			subcontcounter,
			fpsyear
		);


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
	y integer;
BEGIN

	FOR y IN 2016..2027 LOOP

		EXECUTE format(
			'CREATE TABLE fps.period_proj_subcontract_p_y%s
			 PARTITION OF fps.period_proj_subcontract_p
			 FOR VALUES IN (%s)',
			y,
			y
		);

	END LOOP;

END
$$;


CREATE TABLE fps.period_proj_subcontract_p_default
	PARTITION OF fps.period_proj_subcontract_p
	DEFAULT;


-- Preserve existing data.
INSERT INTO fps.period_proj_subcontract_p
SELECT *
FROM fps.period_proj_subcontract;


-- Verify row-count parity before table swap.
DO $$
DECLARE
	v_source_count bigint;
	v_target_count bigint;
BEGIN

	SELECT COUNT(*)
	INTO v_source_count
	FROM fps.period_proj_subcontract;

	SELECT COUNT(*)
	INTO v_target_count
	FROM fps.period_proj_subcontract_p;

	IF v_source_count <> v_target_count THEN

		RAISE EXCEPTION
			'CR083 period_proj_subcontract row-count validation failed: source %, target %.',
			v_source_count,
			v_target_count;

	END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.period_proj_subcontract
	RENAME TO period_proj_subcontract_old;


ALTER TABLE fps.period_proj_subcontract_p
	RENAME TO period_proj_subcontract;


-- ============================================================================
-- 2a. Indexes
-- ============================================================================

CREATE INDEX idx_period_proj_subcontract_project
	ON fps.period_proj_subcontract (project);


CREATE INDEX idx_period_proj_subcontract_period
	ON fps.period_proj_subcontract (period);


CREATE INDEX idx_period_proj_subcontract_fpsyear
	ON fps.period_proj_subcontract (fpsyear);


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
			  'fps.period_proj_subcontract'::regclass
	) THEN

		RAISE EXCEPTION
			'CR083 postcondition failed: fps.period_proj_subcontract is not partitioned.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM fps.period_proj_subcontract
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR083 postcondition failed: period_proj_subcontract contains NULL fpsyear.';

	END IF;

	-- 12 yearly partitions (2016..2027) + DEFAULT = 13.
	SELECT COUNT(*)
	INTO v_partition_count
	FROM pg_inherits
	WHERE inhparent =
		  'fps.period_proj_subcontract'::regclass;

	IF v_partition_count <> 13 THEN

		RAISE EXCEPTION
			'CR083 postcondition failed: expected 13 period_proj_subcontract partitions, found %.',
			v_partition_count;

	END IF;

END
$$;


DROP TABLE fps.period_proj_subcontract_old;


COMMIT;
--
-- Future FPS-year partitions are an infrastructure / DBA responsibility.
-- ============================================================================
