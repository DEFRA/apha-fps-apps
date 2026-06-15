-- 17_refresh_period_tcc.sql
-- Replaces: usp_Refresh_Period_TCC @period
-- Parameter: :period (int)
-- Syntax changes:
--   dbo.Period_TimeCostCalcs -> fps.period_timecostcalcs
--   dbo.TimeCostCalcs        -> fps.timecostcalcs
--   dbo.tlkpProject          -> fps.tlkpproject
--   dbo.CostCentre           -> fps.costcentre
--   dbo.WorkGroup            -> fps.workgroup
--   dbo.tblWGEmployee        -> fps.tblwgemployee
--   [ColumnName]             -> columnname
-- Note: legacy source had a typo '.[dbo].[Period_TimeCostCalcs]' ΓÇö corrected to fps.period_timecostcalcs

DELETE FROM fps.period_timecostcalcs
WHERE period = :period;

INSERT INTO fps.period_timecostcalcs (
    period,
    project,
    oracleprojectcode,
    subaccountcode,
    month,
    defraproject,
    occ,
    opc,
    spc,
    scc,
    name,
    gradecode,
    spnumber,
    chargerate,
    pay,
    nonpay,
    overhead,
    time,
    totalcost
)
SELECT
    :period,
    tlkpproject.parentproject    AS project,
    tlkpproject.oracleprojectcode,
    tlkpproject.subaccountcode,
    timecostcalcs.month,
    CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS defraproject,
    costcentre.costcentre        AS occ,
    costcentre.profitcentre      AS opc,
    workgroup.profitcentre       AS spc,
    workgroup.costcentre         AS scc,
    timecostcalcs.name,
    timecostcalcs.gradecode,
    tblwgemployee.spnumber,
    timecostcalcs.chargerate,
    timecostcalcs.pay,
    timecostcalcs.nonpay,
    timecostcalcs.overhead,
    timecostcalcs.time,
    timecostcalcs.cost           AS totalcost

FROM fps.tblwgemployee
INNER JOIN (
    (fps.tlkpproject
    LEFT JOIN fps.costcentre
        ON tlkpproject.costcentre = costcentre.costcentre)
    INNER JOIN (fps.timecostcalcs
    INNER JOIN fps.workgroup
        ON timecostcalcs.workgroup = workgroup.workgroup)
    ON tlkpproject.parentproject = timecostcalcs.project)
ON tblwgemployee.pactid = timecostcalcs.staffid;
