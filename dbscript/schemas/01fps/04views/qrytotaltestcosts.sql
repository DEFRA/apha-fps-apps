-- View: fps.qrytotaltestcosts
-- Updated 2026-06-17: Enforce year-scoped join to prevent cross-year row amplification
-- See CR014 for cloud DB deployment

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

