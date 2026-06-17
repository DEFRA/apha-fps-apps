-- View: fps.qrytotaltestcosts
-- Updated 2026-06-16: Added fpsyear column for strict year isolation in MABArchive job
-- See CR011 for cloud DB deployment

CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    tr.jobcode,
    p.fpsyear,
    SUM(tr.notests * tr.testprice) AS totaltestcosts
FROM fps.vtbltestrequ tr
INNER JOIN fps.tlkpproject p
    ON p.parentproject = tr.jobcode
GROUP BY
    tr.jobcode,
    p.fpsyear;
