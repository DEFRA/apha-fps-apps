--liquibase formatted sql

--changeset repo-admin:CR015 labels:ddl context:all

DROP VIEW IF EXISTS fps.qrytotaltestcosts;

CREATE VIEW fps.qrytotaltestcosts AS
SELECT
    tr.jobcode,
    tr.fpsyear,
    SUM(tr.notests * tr.testprice) AS totaltestcosts
FROM fps.vtbltestrequ tr
WHERE EXISTS (
    SELECT 1
    FROM fps.tlkpproject p
    WHERE p.parentproject = tr.jobcode
      AND p.fpsyear = tr.fpsyear
)
GROUP BY tr.jobcode, tr.fpsyear;

--ROLLBACK
--Not Applicable
