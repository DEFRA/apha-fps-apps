-- 02_create_fps_totals.sql
-- Replaces: sp_createFPSTotals
-- Syntax changes:
--   dbo.FPSYearTotals        -> fps.fpsyeartotals
--   dbo.tlkpProject          -> fps.tlkpproject
--   dbo.qryTotalAdditionalCosts etc. -> fps views (same names, lower-case)
--   [ColumnName]             -> columnname (unquoted; DB team preserved lower-case)
--   ISNULL(x, 0)             -> COALESCE(x, 0)
-- No formula changes.

INSERT INTO fps.fpsyeartotals
(parentproject, program, totaladditionalcosts, totalanimalcosts, totalstaffcosts, 
 totaltestcosts, totalcosts, custincome, transferincome, totalincome, budget_cvl,
 requiredprofit, manager, customer, projectstatus, pvsincome, plancaseworkdebit, 
 totalpaycosts, fpsyear)
SELECT DISTINCT
    tlkpproject.parentproject,
    tlkpproject.program,

    CASE
        WHEN qrytotaladditionalcosts.totaladditionalcosts IS NULL THEN '0'::money
        ELSE qrytotaladditionalcosts.totaladditionalcosts
    END AS totaladditionalcosts,

    CASE
        WHEN qrytotalanimalcosts.totalanimalcosts IS NULL THEN 0::double precision
        ELSE qrytotalanimalcosts.totalanimalcosts
    END AS totalanimalcosts,

    CASE
        WHEN qrytotalstaffcosts.totalstaffcosts IS NULL THEN 0::double precision
        ELSE qrytotalstaffcosts.totalstaffcosts
    END AS totalstaffcosts,

    CASE
        WHEN qrytotaltestcosts.totaltestcosts IS NULL THEN 0::double precision
        ELSE qrytotaltestcosts.totaltestcosts
    END AS totaltestcosts,

    CASE
        WHEN qrytotaladditionalcosts.totaladditionalcosts IS NULL THEN 0::double precision
        ELSE qrytotaladditionalcosts.totaladditionalcosts::double precision
    END +
    CASE
        WHEN qrytotalanimalcosts.totalanimalcosts IS NULL THEN 0::double precision
        ELSE qrytotalanimalcosts.totalanimalcosts
    END +
    CASE
        WHEN qrytotalstaffcosts.totalstaffcosts IS NULL THEN 0::double precision
        ELSE qrytotalstaffcosts.totalstaffcosts
    END +
    CASE
        WHEN qrytotaltestcosts.totaltestcosts IS NULL THEN 0::double precision
        ELSE qrytotaltestcosts.totaltestcosts
    END +
    CASE
        WHEN tlkpproject.plancaseworkdebit IS NULL THEN 0::double precision
        ELSE tlkpproject.plancaseworkdebit::double precision
    END AS totalcosts,

    tlkpproject.custincome,
    tlkpproject.transferincome,
    custincome + transferincome AS totalincome,
    tlkpproject.budget_cvl,
    tlkpproject.profit AS requiredprofit,
    tlkpproject.manager,
    tlkpproject.customer,
    tlkpproject.projectstatus,

    CASE
        WHEN tlkpproject.pvsincome IS NULL THEN '0'::money
        ELSE tlkpproject.pvsincome
    END AS pvsincome,

    CASE
        WHEN tlkpproject.plancaseworkdebit IS NULL THEN '0'::money
        ELSE tlkpproject.plancaseworkdebit
    END AS plancaseworkdebit,

    CASE
        WHEN qrytotalstaffcosts.totalpaycosts IS NULL THEN 0::double precision
        ELSE qrytotalstaffcosts.totalpaycosts
    END AS totalpaycosts,

    tlkpproject.fpsyear

FROM (((fps.tlkpproject
LEFT JOIN fps.qrytotaladditionalcosts ON tlkpproject.parentproject = qrytotaladditionalcosts.jobcode)
LEFT JOIN fps.qrytotalanimalcosts     ON tlkpproject.parentproject = qrytotalanimalcosts.jobcode)
LEFT JOIN fps.qrytotalstaffcosts      ON tlkpproject.parentproject = qrytotalstaffcosts.jobcode)
LEFT JOIN fps.qrytotaltestcosts       ON tlkpproject.parentproject = qrytotaltestcosts.jobcode;
