-- RecreateSummaries hard-coding fix for localhost
-- Generated on 2026-05-11
-- Target DB: batch_jobs_foundation_db

BEGIN;

-- =====================================================================
-- vtbltestrequ
-- =====================================================================
-- View: fps.vtbltestrequ

CREATE OR REPLACE VIEW fps.vtbltestrequ AS
 SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    datecreated,
    projectbuyercode
   FROM fps.tlkptestreqmt;

-- =====================================================================
-- qrymilestone1
-- =====================================================================
-- View: fps.qrymilestone1

CREATE OR REPLACE VIEW fps.qrymilestone1 AS
 SELECT DISTINCT project,
    milestoneref,
    plandate,
    actualdate,
    monthnofin AS duemonth,
        CASE
            WHEN (actualdate <= plandate) THEN (1)::numeric
            ELSE (0)::numeric
        END AS ontimeflag,
        CASE
            WHEN (actualdate IS NULL) THEN 0
            ELSE 1
        END AS completeflag,
    year,
    fpsyear
   FROM fps.milestone;

COMMIT;
