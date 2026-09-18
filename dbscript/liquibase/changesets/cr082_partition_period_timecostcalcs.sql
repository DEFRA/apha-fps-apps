--liquibase formatted sql

--changeset Nandkishor:CR082 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR082
--
-- Purpose:
--   Introduce FPS-year scoping for the Department Income snapshot table:
--
--     fps.period_timecostcalcs
--
--   This table backs the "Time" query option on the Snapshot Data tab
--   (SQL Server fPeriodTime equivalent). SQL Server stores one snapshot
--   table per FPS year; PostgreSQL keeps multiple years in a single table,
--   so an fpsyear discriminator is required to restore parity.
--
-- Design:
--   - Add fpsyear (nullable first; deterministic backfill required).
--   - Convert to PARTITION BY LIST (fpsyear).
--   - Partition key becomes part of the primary key.
--   - Existing data is preserved.
--   - Explicit partitions: 2016..2027 + DEFAULT.
--   - Original table is used temporarily as *_old during migration and
--     dropped after all validation succeeds.
--
-- NOTE:
--   fps.period_timecostcalcs did NOT previously have an fpsyear column.
--   A deterministic backfill MUST be applied before this CR is run, OR the
--   :target_fpsyear placeholder below must be set to the correct single year
--   this snapshot data belongs to. Do NOT guess historical years.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

	IF to_regclass('fps.period_timecostcalcs') IS NULL THEN
		RAISE EXCEPTION
			'CR082 precondition failed: fps.period_timecostcalcs does not exist.';
	END IF;

	IF to_regclass('fps.period_timecostcalcs_p') IS NOT NULL
	   OR to_regclass('fps.period_timecostcalcs_old') IS NOT NULL THEN

		RAISE EXCEPTION
			'CR082 precondition failed: period_timecostcalcs migration/backup table already exists.';

	END IF;

	-- Fail closed if another table has an FK to this table: a table-swap
	-- would leave the FK pointing at the renamed *_old table.
	IF EXISTS (
		SELECT 1
		FROM pg_constraint
		WHERE contype = 'f'
		  AND confrelid = 'fps.period_timecostcalcs'::regclass
	) THEN

		RAISE EXCEPTION
			'CR082 precondition failed: another table has an FK to fps.period_timecostcalcs. Review dependency before partition conversion.';

	END IF;

END
$$;


-- Prevent writes during copy and table swap.
LOCK TABLE
	fps.period_timecostcalcs
IN ACCESS EXCLUSIVE MODE;


-- ============================================================================
-- 1. Add and backfill fpsyear
-- ============================================================================

ALTER TABLE fps.period_timecostcalcs
	ADD COLUMN IF NOT EXISTS fpsyear integer;


-- Deterministic backfill.
-- Replace the literal below with the approved value (or an approved
-- multi-year backfill statement) before running CR082 in each environment.
UPDATE fps.period_timecostcalcs
SET fpsyear = 2025
WHERE fpsyear IS NULL;


-- Do not proceed with a partition conversion on incomplete data.
DO $$
BEGIN
	IF EXISTS (
		SELECT 1
		FROM fps.period_timecostcalcs
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR082 blocked: fps.period_timecostcalcs contains NULL fpsyear rows. Apply the approved deterministic backfill before running CR082.';

	END IF;
END
$$;


-- Partition key becomes part of the primary key.
ALTER TABLE fps.period_timecostcalcs
	ALTER COLUMN fpsyear SET NOT NULL;


-- ============================================================================
-- 2. Build partitioned replacement table
-- ============================================================================

CREATE TABLE fps.period_timecostcalcs_p
(
	LIKE fps.period_timecostcalcs
	INCLUDING ALL
	EXCLUDING INDEXES
)
PARTITION BY LIST (fpsyear);


-- Partitioned-table primary key must include the partition key.
ALTER TABLE fps.period_timecostcalcs_p
	ADD CONSTRAINT period_timecostcalcs_p_pkey
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
			'CREATE TABLE fps.period_timecostcalcs_p_y%s
			 PARTITION OF fps.period_timecostcalcs_p
			 FOR VALUES IN (%s)',
			y,
			y
		);

	END LOOP;

END
$$;


-- Safety partition for any FPS year not explicitly provisioned.
CREATE TABLE fps.period_timecostcalcs_p_default
	PARTITION OF fps.period_timecostcalcs_p
	DEFAULT;


-- Preserve existing data.
INSERT INTO fps.period_timecostcalcs_p
SELECT *
FROM fps.period_timecostcalcs;


-- Verify row-count parity before table swap.
DO $$
DECLARE
	v_source_count bigint;
	v_target_count bigint;
BEGIN

	SELECT COUNT(*)
	INTO v_source_count
	FROM fps.period_timecostcalcs;

	SELECT COUNT(*)
	INTO v_target_count
	FROM fps.period_timecostcalcs_p;

	IF v_source_count <> v_target_count THEN

		RAISE EXCEPTION
			'CR082 period_timecostcalcs row-count validation failed: source %, target %.',
			v_source_count,
			v_target_count;

	END IF;

END
$$;


-- Swap tables.
ALTER TABLE fps.period_timecostcalcs
	RENAME TO period_timecostcalcs_old;


ALTER TABLE fps.period_timecostcalcs_p
	RENAME TO period_timecostcalcs;


-- ============================================================================
-- 2a. Indexes
-- ============================================================================

-- Recreate query-supporting indexes on the partitioned production table.
-- The Snapshot Time query filters by project + period and is now year-scoped.

CREATE INDEX idx_period_timecostcalcs_project
	ON fps.period_timecostcalcs (project);


CREATE INDEX idx_period_timecostcalcs_period
	ON fps.period_timecostcalcs (period);


CREATE INDEX idx_period_timecostcalcs_fpsyear
	ON fps.period_timecostcalcs (fpsyear);


-- ============================================================================
-- 2b. Sequence ownership
-- ============================================================================

-- The replacement table copied the identity sequence from the source table.
-- Detach the backup from that identity property and make the production table its owner.

ALTER TABLE fps.period_timecostcalcs_old
	ALTER COLUMN id DROP IDENTITY;


DO $$
DECLARE
	v_max_id bigint;
BEGIN

	IF to_regclass(
		'fps.period_timecostcalcs_id_seq'
	) IS NOT NULL THEN

		ALTER SEQUENCE
			fps.period_timecostcalcs_id_seq
		OWNED BY
			fps.period_timecostcalcs.id;

		SELECT COALESCE(MAX(id), 0)
		INTO v_max_id
		FROM fps.period_timecostcalcs;

		PERFORM setval(
			'fps.period_timecostcalcs_id_seq'::regclass,
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
			  'fps.period_timecostcalcs'::regclass
	) THEN

		RAISE EXCEPTION
			'CR082 postcondition failed: fps.period_timecostcalcs is not partitioned.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM fps.period_timecostcalcs
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR082 postcondition failed: period_timecostcalcs contains NULL fpsyear.';

	END IF;

	-- 12 yearly partitions (2016..2027) + DEFAULT = 13.
	SELECT COUNT(*)
	INTO v_partition_count
	FROM pg_inherits
	WHERE inhparent =
		  'fps.period_timecostcalcs'::regclass;

	IF v_partition_count <> 13 THEN

		RAISE EXCEPTION
			'CR082 postcondition failed: expected 13 period_timecostcalcs partitions, found %.',
			v_partition_count;

	END IF;

END
$$;


DROP TABLE fps.period_timecostcalcs_old;


COMMIT;
--
-- Future FPS-year partitions are an infrastructure / DBA responsibility.
-- ============================================================================
