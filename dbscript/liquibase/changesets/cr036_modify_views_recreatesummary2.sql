--liquibase formatted sql

--changeset repo-admin:CR036 labels:ddl context:all

BEGIN;

-- Recreate monthly subcontract totals by project and year.
DROP VIEW IF EXISTS fps.qryjobmonth_subcontracts;

CREATE VIEW fps.qryjobmonth_subcontracts AS
SELECT
    project,
    month::integer AS month,
    fpsyear,
    COALESCE(SUM(animals1::numeric), 0) AS animals,
    COALESCE(SUM(other1::numeric), 0) AS other,
    COALESCE(SUM(animals1::numeric), 0) + COALESCE(SUM(other1::numeric), 0) AS total
FROM fps.qryjobmonth_subcontracts1
GROUP BY
    project,
    month,
    fpsyear;

-- Recreate monthly time, hours, and pay totals by project and year.
DROP VIEW IF EXISTS fps.qryjobmonth_time;

CREATE VIEW fps.qryjobmonth_time AS
SELECT
    project,
    month::integer AS month,
    fpsyear,
    COALESCE(SUM(cost::numeric), 0) AS sumofcost,
    COALESCE(SUM("time"::numeric), 0) AS sumofhours,
    COALESCE(SUM(pay::numeric), 0) AS sumofpayrate
FROM fps.timecostcalcs
GROUP BY
    project,
    month,
    fpsyear;

-- Recreate monthly transfer totals by project and year.
DROP VIEW IF EXISTS fps.qryjobmonth_transferstotal;

CREATE VIEW fps.qryjobmonth_transferstotal AS
SELECT
    project,
    month::integer AS month,
    fpsyear,
    COALESCE(SUM(transfercost::numeric), 0) AS sumoftransfercost
FROM fps.qryjobmonth_transferunion
GROUP BY
    project,
    month,
    fpsyear;

-- Recreate monthly portfolio sales fee totals.
DROP VIEW IF EXISTS fps.qryjobmonthportfoliosales;

CREATE VIEW fps.qryjobmonthportfoliosales AS
SELECT
    tc.planportfolio,
    mo.month::integer AS month,
    mo.fpsyear,
    COALESCE(
        SUM(tr.unitprice::numeric * mo.volume::numeric),
        0
    ) AS fee
FROM fps.tlkptestcapability tc
JOIN fps.monthlyoutput mo
    ON tc.workgroup::text = mo.workgroup::text
    AND tc.testcode::text = mo.testcode::text
    AND tc.fpsyear = mo.fpsyear
JOIN fps.tlkptestreqmt tr
    ON tr.buyer::text = mo.buyer::text
    AND tr.testcode::text = mo.testcode::text
    AND tr.fpsyear = mo.fpsyear
GROUP BY
    tc.planportfolio,
    mo.month,
    mo.fpsyear;

-- Recreate monthly milestone due and completion totals.
DROP VIEW IF EXISTS fps.qryjobmonthmilestone;

CREATE VIEW fps.qryjobmonthmilestone AS
SELECT
    project,
    duemonth::integer AS duemonth,
    fpsyear,
    COUNT(milestoneref) AS mstonedue,
    COALESCE(SUM(completeflag), 0) AS due__done,
    COALESCE(SUM(ontimeflag::numeric), 0) AS ontime
FROM fps.qrymilestone1
GROUP BY
    project,
    duemonth,
    fpsyear;

COMMIT;

--ROLLBACK
--Not Applicable