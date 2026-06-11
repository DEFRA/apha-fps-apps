-- 15_refresh_period_mo.sql
-- Replaces: usp_Refresh_Period_MO @period
-- Parameter: :period (int)
-- Syntax changes:
--   dbo.Period_MonthlyOutput -> fps.period_monthlyoutput
--   dbo.tlkpProject          -> fps.tlkpproject
--   dbo.CostCentre           -> fps.costcentre
--   dbo.MonthlyOutput        -> fps.monthlyoutput
--   dbo.WorkGroup            -> fps.workgroup
--   dbo.tlkpTestReqmt        -> fps.tlkptestreqmt
--   CONVERT(money, expr)     -> CAST(expr AS numeric)
--   [ColumnName]             -> columnname

DELETE FROM fps.period_monthlyoutput
WHERE period = :period;

INSERT INTO fps.period_monthlyoutput (
    period,
    project,
    oracleprojectcode,
    subaccountcode,
    isdefraproject,
    opc,
    occ,
    month,
    spc,
    workgroup,
    scc,
    testcode,
    volume,
    testprice,
    totalcost
)
SELECT
    :period,
    tlkpproject.parentproject    AS project,
    tlkpproject.oracleprojectcode,
    tlkpproject.subaccountcode,
    CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS isdefraproject,
    costcentre.profitcentre      AS opc,
    costcentre.costcentre        AS occ,
    monthlyoutput.month,
    workgroup.profitcentre       AS spc,
    workgroup.workgroup,
    workgroup.costcentre         AS scc,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice      AS testprice,
    CAST(unitprice * volume AS numeric) AS totalcost

FROM ((fps.tlkpproject
LEFT JOIN fps.costcentre
    ON tlkpproject.costcentre = costcentre.costcentre)
    INNER JOIN (fps.monthlyoutput
    INNER JOIN fps.workgroup
        ON monthlyoutput.workgroup = workgroup.workgroup)
    ON tlkpproject.parentproject = monthlyoutput.buyer)
    INNER JOIN fps.tlkptestreqmt
        ON  monthlyoutput.buyer     = tlkptestreqmt.projectbuyercode
        AND monthlyoutput.testcode  = tlkptestreqmt.testcode;
