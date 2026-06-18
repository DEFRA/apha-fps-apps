-- View: fps.qrytotaltestcosts_refined_validation
-- Added 2026-06-18: Temporary secondary view for CR015 proof run
-- Purpose: Side-by-side comparison with fps.qrytotaltestcosts before cutover

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
