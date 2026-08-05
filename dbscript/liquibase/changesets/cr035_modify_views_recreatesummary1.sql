--liquibase formatted sql

--changeset repo-admin:CR035 labels:ddl context:all

BEGIN;

-- Recreate total animal costs by job and year.
DROP VIEW IF EXISTS fps.qrytotalanimalcosts;

CREATE VIEW fps.qrytotalanimalcosts AS
SELECT
    parentproject AS jobcode,
    fpsyear,
    COALESCE(SUM(cost::numeric), 0) AS totalanimalcosts
FROM fps.vprojectanimalplan
GROUP BY
    parentproject,
    fpsyear;

-- Recreate total staff and pay costs by job and year.
DROP VIEW IF EXISTS fps.qrytotalstaffcosts;

CREATE VIEW fps.qrytotalstaffcosts AS
SELECT
    parentproject AS jobcode,
    fpsyear,
    COALESCE(SUM(cost::numeric), 0) AS totalstaffcosts,
    COALESCE(SUM(paycost::numeric), 0) AS totalpaycosts
FROM fps.vprojectstaffplan
GROUP BY
    parentproject,
    fpsyear;

-- Recreate total test costs by job and year.
DROP VIEW IF EXISTS fps.qrytotaltestcosts;

CREATE VIEW fps.qrytotaltestcosts AS
SELECT
    tr.buyer AS jobcode,
    tr.fpsyear,
    COALESCE(
        SUM(tr.norequired::numeric * tr.unitprice::numeric),
        0
    ) AS totaltestcosts
FROM fps.tlkptestreqmt tr
JOIN fps.tlkpproject p
    ON p.parentproject::text = tr.buyer::text
    AND p.fpsyear = tr.fpsyear
GROUP BY
    tr.buyer,
    tr.fpsyear;

COMMIT;

--ROLLBACK
--Not Applicable