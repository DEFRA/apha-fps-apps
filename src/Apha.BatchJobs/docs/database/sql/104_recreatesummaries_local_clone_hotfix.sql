-- 104_recreatesummaries_local_clone_hotfix.sql
-- Purpose:
--   Local clone hotfix for RecreateSummaries validation on consolidated PostgreSQL.
--
-- IMPORTANT:
--   1) This script is intended for local cloned environments only.
--   2) It applies an operational fallback for ambiguous multi-year projectmonth rows:
--      when projectmonth.fpsyear is NULL and a project exists in multiple years,
--      this script assigns MAX(fpsyear).
--   3) Production use requires explicit DBA/business approval of the backfill rule.

BEGIN;

-- -----------------------------------------------------------------------------
-- A. Remove UT residue rows from prior test runs (project id/code patterns like UT%)
-- -----------------------------------------------------------------------------
DELETE FROM fps.period_timecostcalcs WHERE project::text LIKE 'UT%';
DELETE FROM fps.period_proj_subcontract WHERE project::text LIKE 'UT%';
DELETE FROM fps.period_monthlyoutput WHERE project::text LIKE 'UT%';
DELETE FROM fps.projectmonthfinal WHERE project::text LIKE 'UT%';
DELETE FROM fps.projectmonthcasework WHERE project::text LIKE 'UT%';
DELETE FROM fps.projectmonth3 WHERE project::text LIKE 'UT%';
DELETE FROM fps.projectmonth2 WHERE project::text LIKE 'UT%';
DELETE FROM fps.fpsyeartotals WHERE parentproject::text LIKE 'UT%';
DELETE FROM fps.timecostcalcs WHERE project::text LIKE 'UT%';
DELETE FROM fps.proj_invoice WHERE projectparent::text LIKE 'UT%';
DELETE FROM fps.proj_subcontract WHERE project::text LIKE 'UT%';
DELETE FROM fps.projectmonth WHERE project::text LIKE 'UT%';
DELETE FROM fps.monthlyoutput
WHERE buyer::text LIKE 'UT%'
   OR workgroup::text LIKE 'UT%'
   OR testcode::text LIKE 'UT%';
DELETE FROM fps.testorproduct WHERE itemcode::text LIKE 'UT%';
DELETE FROM fps.tlkptestcapability
WHERE testcode::text LIKE 'UT%'
   OR workgroup::text LIKE 'UT%'
   OR planportfolio::text LIKE 'UT%';
DELETE FROM fps.tlkptestreqmt
WHERE buyer::text LIKE 'UT%'
   OR projectbuyercode::text LIKE 'UT%'
   OR testcode::text LIKE 'UT%';
DELETE FROM fps.milestone WHERE project::text LIKE 'UT%';
DELETE FROM fps.timecodevalid
WHERE parentproject::text LIKE 'UT%'
   OR workgroup::text LIKE 'UT%'
   OR timecode::text LIKE 'UT%';
DELETE FROM fps.tblstaffjob WHERE jobcode::text LIKE 'UT%' OR staffid::text LIKE 'UT%';
DELETE FROM fps.tblanimalreq WHERE jobcode::text LIKE 'UT%' OR animaltype::text LIKE 'UT%';
DELETE FROM fps.tbladditionalcosts WHERE jobcode::text LIKE 'UT%';
DELETE FROM fps.monthlytime
WHERE pactstaffid::text LIKE 'UT%'
   OR parentproject::text LIKE 'UT%'
   OR workgroup::text LIKE 'UT%'
   OR timecode::text LIKE 'UT%';
DELETE FROM fps.tblwgemployee
WHERE pactid::text LIKE 'UT%'
   OR spnumber::text LIKE 'UT%'
   OR workgroupgrade::text LIKE 'UT%';
DELETE FROM fps.tblemployee WHERE spnumber::text LIKE 'UT%';
DELETE FROM fps.workgroupgrade
WHERE wggrade::text LIKE 'UT%'
   OR profitcentregrade::text LIKE 'UT%'
   OR workgroup::text LIKE 'UT%';
DELETE FROM fps.profitcentregrade
WHERE pcgrade::text LIKE 'UT%'
   OR profitcentre::text LIKE 'UT%';
DELETE FROM fps.workgroup
WHERE workgroup::text LIKE 'UT%'
   OR profitcentre::text LIKE 'UT%';
DELETE FROM fps.costcentre WHERE profitcentre::text LIKE 'UT%';
DELETE FROM fps.tblkpprofitcentre WHERE profitcentre::text LIKE 'UT%';
DELETE FROM fps.tblanimals WHERE animaltype::text LIKE 'UT%';
DELETE FROM fps.tlkpproject WHERE parentproject::text LIKE 'UT%';
DELETE FROM fps.tlkpprogram WHERE programno::text LIKE 'UT%';

-- -----------------------------------------------------------------------------
-- B. Backfill NULL fpsyear in projectmonth
-- -----------------------------------------------------------------------------
-- B1) Deterministic rows: projects with exactly one year in tlkpproject
WITH one_year AS (
    SELECT parentproject::text AS project, MIN(fpsyear) AS fpsyear
    FROM fps.tlkpproject
    GROUP BY parentproject
    HAVING COUNT(DISTINCT fpsyear) = 1
)
UPDATE fps.projectmonth pm
SET fpsyear = oy.fpsyear
FROM one_year oy
WHERE pm.fpsyear IS NULL
  AND pm.project::text = oy.project;

-- B2) Ambiguous rows: projects with multiple years in tlkpproject
-- Local operational fallback: assign latest year (MAX)
WITH max_year AS (
    SELECT parentproject::text AS project, MAX(fpsyear) AS fpsyear
    FROM fps.tlkpproject
    GROUP BY parentproject
)
UPDATE fps.projectmonth pm
SET fpsyear = my.fpsyear
FROM max_year my
WHERE pm.fpsyear IS NULL
  AND pm.project::text = my.project;

-- B3) Guard: block commit if NULLs remain
DO $$
DECLARE
    v_missing int;
BEGIN
    SELECT COUNT(*) INTO v_missing
    FROM fps.projectmonth
    WHERE fpsyear IS NULL;

    IF v_missing > 0 THEN
        RAISE EXCEPTION 'projectmonth fpsyear backfill incomplete: % rows still null', v_missing;
    END IF;
END $$;

-- -----------------------------------------------------------------------------
-- C. Enforce local integrity guardrail
-- -----------------------------------------------------------------------------
ALTER TABLE fps.projectmonth
    ALTER COLUMN fpsyear SET NOT NULL;

COMMIT;
