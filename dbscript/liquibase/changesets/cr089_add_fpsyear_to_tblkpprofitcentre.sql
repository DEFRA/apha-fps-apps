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
-- Design:
--   - Add fpsyear (nullable first). A separate data-migration program
--     populates one row per applicable (profitcentre, fpsyear) pair; this
--     changeset does not choose or copy the year-specific values.
--   - Enforce fpsyear NOT NULL and add an FK to fps.tblyearmaster.
--   - Replace the single-column primary key (profitcentre) with a composite
--     primary key (profitcentre, fpsyear).
--   - Replace the five existing inbound foreign keys with composite
--     (profitcentre, fpsyear) foreign keys, and add the missing FK from
--     fps.tbluser_profitcentre on the same pair.
--   - Recreate the dependent views that join to fps.tblkpprofitcentre so the
--     join includes fpsyear, preventing cross-year row multiplication.
--
-- Out of scope:
--   - Yearly partition conversion (PARTITION BY LIST (fpsyear)) for
--     fps.tblkpprofitcentre remains a separate later changeset, consistent
--     with the shadow-table pattern used for other partitioned tables in
--     this repository.
--
-- NOTE:
--   fps.tblkpprofitcentre did NOT previously have an fpsyear column. The
--   separate data-migration program MUST populate fpsyear for every row
--   (including inserting one row per additional applicable year) before the
--   NOT NULL / key / FK steps below can succeed.
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
-- 1. Add fpsyear (nullable)
-- ============================================================================

ALTER TABLE fps.tblkpprofitcentre
	ADD COLUMN IF NOT EXISTS fpsyear integer;

COMMENT ON COLUMN fps.tblkpprofitcentre.fpsyear IS
	'FPS year. One row exists per (profitcentre, fpsyear) once the data-migration program has run.';


-- ============================================================================
-- 2. Require the data-migration program's output before proceeding
-- ============================================================================

DO $$
BEGIN
	IF EXISTS (
		SELECT 1
		FROM fps.tblkpprofitcentre
		WHERE fpsyear IS NULL
	) THEN

		RAISE EXCEPTION
			'CR089 blocked: fps.tblkpprofitcentre contains NULL fpsyear rows. Run the fpsyear data-migration program before applying the CR089 key/FK/view changes.';

	END IF;

	IF EXISTS (
		SELECT profitcentre, fpsyear
		FROM fps.tblkpprofitcentre
		GROUP BY profitcentre, fpsyear
		HAVING COUNT(*) > 1
	) THEN

		RAISE EXCEPTION
			'CR089 blocked: fps.tblkpprofitcentre contains duplicate (profitcentre, fpsyear) rows.';

	END IF;

	IF EXISTS (
		SELECT 1
		FROM fps.tblkpprofitcentre pc
		WHERE NOT EXISTS (
			SELECT 1
			FROM fps.tblyearmaster ym
			WHERE ym.fpsyear = pc.fpsyear
		)
	) THEN

		RAISE EXCEPTION
			'CR089 blocked: fps.tblkpprofitcentre contains fpsyear values not present in fps.tblyearmaster.';

	END IF;
END
$$;


-- ============================================================================
-- 3. Enforce NOT NULL and add the FPS-year FK
-- ============================================================================

ALTER TABLE fps.tblkpprofitcentre
	ALTER COLUMN fpsyear SET NOT NULL;

DO $$
BEGIN
	IF NOT EXISTS (
		SELECT 1
		FROM pg_constraint
		WHERE conname = 'fk_tblkpprofitcentre_fpsyear'
		  AND conrelid = 'fps.tblkpprofitcentre'::regclass
	) THEN
		ALTER TABLE fps.tblkpprofitcentre
			ADD CONSTRAINT fk_tblkpprofitcentre_fpsyear
				FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster (fpsyear);
	END IF;
END
$$;


-- ============================================================================
-- 4. Replace the single-column primary key with (profitcentre, fpsyear)
-- ============================================================================

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


-- ============================================================================
-- 5. Replace inbound foreign keys with composite (profitcentre, fpsyear)
-- ============================================================================

-- fps.costcentre
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_costcentre_profitcentre'
		  AND conrelid = 'fps.costcentre'::regclass
	) THEN
		ALTER TABLE fps.costcentre DROP CONSTRAINT fk_costcentre_profitcentre;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_costcentre_profitcentre_fpsyear'
		  AND conrelid = 'fps.costcentre'::regclass
	) THEN
		ALTER TABLE fps.costcentre
			ADD CONSTRAINT fk_costcentre_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;

-- fps.profitcentregrade
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_profitcentre'
		  AND conrelid = 'fps.profitcentregrade'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade DROP CONSTRAINT fk_profitcentregrade_profitcentre;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_profitcentre_fpsyear'
		  AND conrelid = 'fps.profitcentregrade'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade
			ADD CONSTRAINT fk_profitcentregrade_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;

-- fps.profitcentregrade_nondefra
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_nondefra_profitcentre'
		  AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade_nondefra DROP CONSTRAINT fk_profitcentregrade_nondefra_profitcentre;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_profitcentregrade_nondefra_profitcentre_fpsyear'
		  AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
	) THEN
		ALTER TABLE fps.profitcentregrade_nondefra
			ADD CONSTRAINT fk_profitcentregrade_nondefra_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;

-- fps.tbltestrccost
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_tbltestrccost_profitcentre'
		  AND conrelid = 'fps.tbltestrccost'::regclass
	) THEN
		ALTER TABLE fps.tbltestrccost DROP CONSTRAINT fk_tbltestrccost_profitcentre;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_tbltestrccost_profitcentre_fpsyear'
		  AND conrelid = 'fps.tbltestrccost'::regclass
	) THEN
		ALTER TABLE fps.tbltestrccost
			ADD CONSTRAINT fk_tbltestrccost_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;

-- fps.workgroup
DO $$
BEGIN
	IF EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_workgroup_profitcentre'
		  AND conrelid = 'fps.workgroup'::regclass
	) THEN
		ALTER TABLE fps.workgroup DROP CONSTRAINT fk_workgroup_profitcentre;
	END IF;

	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_workgroup_profitcentre_fpsyear'
		  AND conrelid = 'fps.workgroup'::regclass
	) THEN
		ALTER TABLE fps.workgroup
			ADD CONSTRAINT fk_workgroup_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;

-- fps.tbluser_profitcentre (new FK; none existed against the parent before)
DO $$
BEGIN
	IF NOT EXISTS (
		SELECT 1 FROM pg_constraint
		WHERE conname = 'fk_tbluser_profitcentre_profitcentre_fpsyear'
		  AND conrelid = 'fps.tbluser_profitcentre'::regclass
	) THEN
		ALTER TABLE fps.tbluser_profitcentre
			ADD CONSTRAINT fk_tbluser_profitcentre_profitcentre_fpsyear
				FOREIGN KEY (profitcentre, fpsyear) REFERENCES fps.tblkpprofitcentre (profitcentre, fpsyear);
	END IF;
END
$$;


-- ============================================================================
-- 6. Recreate dependent views with fpsyear-aware joins
-- ============================================================================
-- Only the join/select changes needed because fps.tblkpprofitcentre is now
-- year-scoped are made here. Column lists for existing views are preserved
-- (fpsyear is appended where it was previously absent) so consumers relying
-- on the current output contract are not broken.

-- View: fps.vtblkpprofitcentre (originally created by CR024)
CREATE OR REPLACE VIEW fps.vtblkpprofitcentre
AS
SELECT DISTINCT
    pc.profitcentre,
    pc.profitcentrename,
    pc.division,
    pc.conttarget,
    pc.profitcentrehead,
    pc.divisionid,
    pc.email_recipient,
    pc.highlevelsummary,
    u.user_id,
    u.dt2username,
    u.useremail,
    upc.fpsyear
FROM fps.tblkpprofitcentre pc
INNER JOIN fps.tbluser_profitcentre upc
    ON pc.profitcentre::text = upc.profitcentre::text
   AND pc.fpsyear = upc.fpsyear
INNER JOIN fps.tblusers u
    ON upc.user_id = u.user_id;

-- View: fps.vpacttblkpprofitcentre
CREATE OR REPLACE VIEW fps.vpacttblkpprofitcentre AS
SELECT
    profitcentre,
    profitcentrename,
    division,
    conttarget,
    profitcentrehead,
    divisionid,
    email_recipient,
    pactcoordinatoremailname,
    timesheet,
    outputsheet,
    timesheetlayout,
    fpsyear
FROM fps.tblkpprofitcentre;

-- View: fps.vtblkpprofitcentre_general
CREATE OR REPLACE VIEW fps.vtblkpprofitcentre_general AS
SELECT
    profitcentre,
    profitcentrename,
    fpsyear
FROM fps.tblkpprofitcentre;

-- View: fps.vqrytbidsum
CREATE OR REPLACE VIEW fps.vqrytbidsum AS
SELECT
    pc.profitcentre,
    b.fpsyear,
    sum(b.genbid) AS sumofgenbid,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
JOIN fps.workgroup w
    ON pc.profitcentre::text = w.profitcentre::text
   AND pc.fpsyear = w.fpsyear
JOIN fps.tblbid b
    ON w.workgroup::text = b.workgroup::text
   AND w.fpsyear = b.fpsyear
JOIN fps.tbluser_profitcentre upc
    ON pc.profitcentre::text = upc.profitcentre::text
   AND pc.fpsyear = upc.fpsyear
JOIN fps.tblusers u
    ON upc.user_id = u.user_id
GROUP BY pc.profitcentre, b.fpsyear, u.user_id, u.dt2username, u.useremail;

-- View: fps.vprofitcentregrade
CREATE OR REPLACE VIEW fps.vprofitcentregrade AS
SELECT DISTINCT
    pcg.pcgrade,
    pcg.divisiongrade,
    pcg.gradecode,
    pcg.profitcentre,
    pcg.chargerate,
    pcg.directrate,
    pcg.payrate,
    pcg.npr,
    pcg.ohr,
    pcg.hrsavailable,
    pcg.oldchargerate,
    pcg.defrachargerate,
    pcg.fpsyear,
    vpc.user_id,
    vpc.dt2username,
    vpc.useremail
FROM fps.profitcentregrade pcg
JOIN fps.vtblkpprofitcentre vpc
    ON pcg.profitcentre::text = vpc.profitcentre::text
   AND pcg.fpsyear = vpc.fpsyear;

-- View: fps.vtblpurchase
CREATE OR REPLACE VIEW fps.vtblpurchase AS
SELECT DISTINCT
    tp.workgroup,
    tp.account,
    tp.itemdescription,
    tp.amount,
    tp.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tblpurchase tp
JOIN fps.tblbid b
    ON tp.workgroup::text = b.workgroup::text
JOIN fps.workgroup w
    ON b.workgroup::text = w.workgroup::text
JOIN fps.tblkpprofitcentre pc
    ON w.profitcentre::text = pc.profitcentre::text
   AND w.fpsyear = pc.fpsyear
JOIN fps.tbluser_profitcentre upc
    ON pc.profitcentre::text = upc.profitcentre::text
   AND pc.fpsyear = upc.fpsyear
JOIN fps.tblusers u
    ON upc.user_id = u.user_id
WHERE tp.account::text IN (
    SELECT b2.account
    FROM fps.tblbid b2
    JOIN fps.workgroup w2
        ON b2.workgroup::text = w2.workgroup::text
    JOIN fps.tblkpprofitcentre pc2
        ON w2.profitcentre::text = pc2.profitcentre::text
       AND w2.fpsyear = pc2.fpsyear
    JOIN fps.tbluser_profitcentre upc2
        ON pc2.profitcentre::text = upc2.profitcentre::text
       AND pc2.fpsyear = upc2.fpsyear
    WHERE upc2.user_id = u.user_id
);

-- View: fps.qryfrmtimesellerpc_map (originally modified by CR040)
DROP VIEW IF EXISTS fps.qryfrmtimesellerpc_map;
CREATE OR REPLACE VIEW fps.qryfrmtimesellerpc_map AS
SELECT tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre AS sellingpc,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade AS profitcentregrade_col,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours AS apphours,
    sum(vstaffjobhours.plannedhours) AS hrs,
    sum(tblwgemployee.hrsavail) AS avhrs,
    sum(vstaffjobhours.plannedhours) * profitcentregrade.chargerate AS fec,
    vapphours.sumofplannedhours * profitcentregrade.chargerate AS appfec,
    profitcentregrade.ohr * sum(vstaffjobhours.plannedhours) AS contribution
FROM fps.vapphours
    RIGHT JOIN (
        fps.tblkpprofitcentre
        JOIN (
            fps.profitcentregrade
            LEFT JOIN fps.vqrytbidsum ON profitcentregrade.profitcentre::text = vqrytbidsum.profitcentre::text
        ) ON tblkpprofitcentre.profitcentre::text = profitcentregrade.profitcentre::text
        AND tblkpprofitcentre.fpsyear = profitcentregrade.fpsyear
        JOIN fps.workgroupgrade ON profitcentregrade.pcgrade::text = workgroupgrade.profitcentregrade::text
        JOIN fps.tblwgemployee ON workgroupgrade.wggrade::text = tblwgemployee.workgroupgrade::text
    ) ON vapphours.workgroupgrade::text = workgroupgrade.wggrade::text
    LEFT JOIN fps.vstaffjobhours ON tblwgemployee.pactid::text = vstaffjobhours.staffid::text
GROUP BY tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours;

-- View: fps.vqryfrmtimesellerpc (originally modified by CR040)
DROP VIEW IF EXISTS fps.vqryfrmtimesellerpc;
CREATE OR REPLACE VIEW fps.vqryfrmtimesellerpc AS
SELECT pc.conttarget,
    pcg.profitcentre AS sellingpc,
    pcg.chargerate,
    pcg.ohr,
    bsum.sumofgenbid,
    wgg.workgroup,
    wgg.profitcentregrade,
    wgg.wggrade,
    ah.sumofplannedhours AS apphours,
    sum(sjh.plannedhours) AS hrs,
    sum(we.hrsavail) AS avhrs,
    (sum(sjh.plannedhours) * pcg.chargerate)::numeric AS fec,
    (ah.sumofplannedhours * pcg.chargerate)::numeric AS appfec,
    (pcg.ohr * sum(sjh.plannedhours))::numeric AS contribution,
    we.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
    JOIN fps.tbluser_profitcentre upc ON pc.profitcentre::text = upc.profitcentre::text
    AND pc.fpsyear = upc.fpsyear
    JOIN fps.tblusers u ON upc.user_id = u.user_id
    JOIN fps.profitcentregrade pcg ON pc.profitcentre::text = pcg.profitcentre::text
    AND pc.fpsyear = pcg.fpsyear
    LEFT JOIN fps.vqrytbidsum bsum ON pcg.profitcentre::text = bsum.profitcentre::text
    AND pcg.fpsyear = bsum.fpsyear
    AND u.user_id = bsum.user_id
    JOIN fps.workgroupgrade wgg ON pcg.pcgrade::text = wgg.profitcentregrade::text
    AND pcg.fpsyear = wgg.fpsyear
    JOIN fps.tblwgemployee we ON wgg.wggrade::text = we.workgroupgrade::text
    AND wgg.fpsyear = we.fpsyear
    LEFT JOIN fps.vapphours ah ON wgg.wggrade::text = ah.workgroupgrade::text
    AND wgg.fpsyear = ah.fpsyear
    LEFT JOIN fps.vstaffjobhours sjh ON we.pactid::text = sjh.staffid::text
    AND we.fpsyear = sjh.fpsyear
GROUP BY pc.conttarget,
    pcg.profitcentre,
    pcg.chargerate,
    pcg.ohr,
    bsum.sumofgenbid,
    wgg.workgroup,
    wgg.profitcentregrade,
    wgg.wggrade,
    ah.sumofplannedhours,
    we.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail;

-- fps.vworkgroup (CR025) already matches on w.fpsyear = vpc.fpsyear and
-- requires no change now that vtblkpprofitcentre is fixed above.

COMMIT;

--rollback empty ;