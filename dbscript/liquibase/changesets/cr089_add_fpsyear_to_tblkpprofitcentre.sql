--liquibase formatted sql

--changeset repo-admin:CR089 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR089
--
-- Purpose:
--   Introduce FPS-year scoping for fps.tblkpprofitcentre so the same
--   profitcentre code can have one row per applicable FPS year, matching the
--   year-scoped relationships that already exist on costcentre,
--   profitcentregrade, profitcentregrade_nondefra, tbltestrccost, workgroup,
--   and tbluser_profitcentre.
--
-- This changeset runs BEFORE the separate data-migration program and puts
-- the final composite primary key (profitcentre, fpsyear) in place
-- immediately, instead of a temporary/interim key:
--   1. Add fpsyear NOT NULL DEFAULT -1. -1 is a sentinel meaning "not yet
--      migrated"; every existing row gets it automatically, and it cannot
--      collide with any other existing row because profitcentre was already
--      unique under the old primary key.
--   2. Drop the five old inbound foreign keys and the single-column primary
--      key, then add the composite primary key (profitcentre, fpsyear)
--      straight away. Because every row already has a value (the sentinel),
--      this succeeds without a duplicate-key violation, and from this point
--      on the primary key itself is what stops any duplicate
--      (profitcentre, fpsyear) row from being inserted.
--
-- The data-migration program then replaces the sentinel rows: for each
-- profitcentre it inserts/updates rows so there is one row per applicable
-- FPS year, and no row is left with fpsyear = -1.
--
-- postdbscripts/cr089_tblkpprofitcentre_fpsyear_cutover.sql runs after the
-- data-migration program. It validates that no sentinel rows remain, adds
-- the FK to fps.tblyearmaster, re-adds the five foreign keys (plus the new
-- tbluser_profitcentre FK) against the composite key, and recreates the
-- dependent views.
--
-- Out of scope:
--   - Yearly partition conversion (PARTITION BY LIST (fpsyear)) for
--     fps.tblkpprofitcentre remains a separate later change, consistent
--     with the shadow-table pattern used for other partitioned tables in
--     this repository.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN
	IF EXISTS (
		SELECT 1
		FROM pg_constraint
		WHERE conname = 'pk_tblkpprofitcentre'
		  AND conrelid = 'fps.tblkpprofitcentre'::regclass
	) THEN

		RAISE EXCEPTION
			'CR089 precondition failed: fps.tblkpprofitcentre already has the composite primary key. CR089 appears to have already run.';

	END IF;
END
$$;


-- ============================================================================
-- 1. Add fpsyear (sentinel default, NOT NULL from the start)
-- ============================================================================

ALTER TABLE fps.tblkpprofitcentre
	ADD COLUMN IF NOT EXISTS fpsyear integer NOT NULL DEFAULT -1;

COMMENT ON COLUMN fps.tblkpprofitcentre.fpsyear IS
	'FPS year. -1 is a sentinel meaning "not yet migrated"; the data-migration program replaces every -1 row with one row per applicable FPS year.';


-- ============================================================================
-- 2. Drop old inbound foreign keys that depend on the single-column PK index
-- ============================================================================
-- These must be dropped before the primary key itself, otherwise Postgres
-- refuses to drop the PK because it backs these constraints' index. They
-- are re-added against the composite key by the postdbscripts cutover
-- script, once the migration program has finished.

DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_costcentre_profitcentre'
		  AND conrelid = 'fps.costcentre'::regclass
	) THEN
		ALTER TABLE fps.costcentre DROP CONSTRAINT fk_costcentre_profitcentre;
	END IF;
END
$$;

DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_profitcentre'
		  AND conrelid = 'fps.profitcentregrade'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade DROP CONSTRAINT fk_profitcentregrade_profitcentre;
	END IF;
END
$$;

DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_nondefra_profitcentre'
		  AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade_nondefra DROP CONSTRAINT fk_profitcentregrade_nondefra_profitcentre;
	END IF;
END
$$;

DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_tbltestrccost_profitcentre'
		  AND conrelid = 'fps.tbltestrccost'::regclass
	) THEN
		ALTER TABLE fps.tbltestrccost DROP CONSTRAINT fk_tbltestrccost_profitcentre;
	END IF;
END
$$;

DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_workgroup_profitcentre'
		  AND conrelid = 'fps.workgroup'::regclass
	) THEN
		ALTER TABLE fps.workgroup DROP CONSTRAINT fk_workgroup_profitcentre;
	END IF;
END
$$;


-- ============================================================================
-- 3. Replace the single-column primary key with (profitcentre, fpsyear)
-- ============================================================================
-- Every existing row already has a value for fpsyear (the -1 sentinel), and
-- those (profitcentre, -1) pairs are all unique because profitcentre was
-- already unique under the old primary key, so this succeeds immediately -
-- no interim unique key is needed.

DO $$
DECLARE
	v_existing_pk_name text;
BEGIN
	SELECT conname
	INTO v_existing_pk_name
	FROM pg_constraint
	WHERE conrelid = 'fps.tblkpprofitcentre'::regclass
	  AND contype = 'p';

	IF v_existing_pk_name IS NOT NULL THEN
		EXECUTE format(
			'ALTER TABLE fps.tblkpprofitcentre DROP CONSTRAINT %I',
			v_existing_pk_name
		);
	END IF;
END
$$;

ALTER TABLE fps.tblkpprofitcentre
	ADD CONSTRAINT pk_tblkpprofitcentre PRIMARY KEY (profitcentre, fpsyear);

COMMIT;

--rollback empty ;