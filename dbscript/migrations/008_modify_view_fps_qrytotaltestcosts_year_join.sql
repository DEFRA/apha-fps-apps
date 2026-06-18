-- CR014: Fix fps.qrytotaltestcosts year-scoped join to prevent cross-year amplification
-- Date: 2026-06-17
-- Request Type: Modify
-- Schema: fps
-- Object Type: View

BEGIN;

CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    tr.jobcode,
    tr.fpsyear,
    SUM(tr.notests * tr.testprice) AS totaltestcosts
FROM fps.vtbltestrequ tr
JOIN fps.tlkpproject p
    ON p.parentproject = tr.jobcode
   AND p.fpsyear = tr.fpsyear
GROUP BY tr.jobcode, tr.fpsyear;

COMMIT;
