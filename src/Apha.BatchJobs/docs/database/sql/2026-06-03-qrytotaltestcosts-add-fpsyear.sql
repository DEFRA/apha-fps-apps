-- MABArchive strict-year-isolation prerequisite fix
-- Date: 2026-06-03
-- Purpose: Ensure fps.qrytotaltestcosts exposes fpsyear so strict year-aware joins can execute.

CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    jobcode,
    SUM(notests * testprice) AS totaltestcosts,
    fpsyear
FROM fps.vtbltestrequ
GROUP BY jobcode, fpsyear;
