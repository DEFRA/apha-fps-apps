-- 16_refresh_period_psc.sql
-- Replaces: usp_Refresh_Period_PSC @period
-- Parameter: :period (int)
-- Syntax changes:
--   dbo.Period_Proj_SubContract -> fps.period_proj_subcontract
--   dbo.Proj_SubContract        -> fps.proj_subcontract
--   dbo.tlkpProject             -> fps.tlkpproject
--   dbo.CostCentre              -> fps.costcentre
--   [ColumnName]                -> columnname

DELETE FROM fps.period_proj_subcontract
WHERE period = :period;

INSERT INTO fps.period_proj_subcontract (
    period,
    subcontcounter,
    project,
    oracleprojectcode,
    subaccountcode,
    isdefraproject,
    opc,
    occ,
    month,
    amount,
    acctcode
)
SELECT
    :period,
    proj_subcontract.subcontcounter,
    proj_subcontract.project,
    tlkpproject.oracleprojectcode,
    tlkpproject.subaccountcode,
    CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS isdefraproject,
    costcentre.profitcentre  AS opc,
    costcentre.costcentre    AS occ,
    proj_subcontract.month,
    proj_subcontract.amount,
    proj_subcontract.acctcode

FROM fps.costcentre
RIGHT OUTER JOIN fps.tlkpproject
    ON fps.costcentre.costcentre = fps.tlkpproject.costcentre
INNER JOIN fps.proj_subcontract
    ON fps.tlkpproject.parentproject = fps.proj_subcontract.project;
