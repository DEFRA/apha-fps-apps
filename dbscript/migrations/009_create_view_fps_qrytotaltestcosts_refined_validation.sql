-- CR015: Create secondary validation view to evaluate refined test-cost totals logic
-- Date: 2026-06-18
-- Request Type: Create
-- Schema: fps
-- Object Type: View

BEGIN;

CREATE OR REPLACE VIEW fps.qrytotaltestcosts_refined_validation AS
SELECT
    tr.buyer AS jobcode,
    tr.fpsyear,
    SUM(tr.norequired * tr.unitprice) AS totaltestcosts
FROM fps.tlkptestreqmt tr
JOIN fps.tlkpproject p
    ON p.parentproject = tr.buyer
   AND p.fpsyear = tr.fpsyear
GROUP BY tr.buyer, tr.fpsyear;

COMMIT;
