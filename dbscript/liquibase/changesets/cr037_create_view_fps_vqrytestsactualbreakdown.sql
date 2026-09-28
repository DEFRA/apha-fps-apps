--liquibase formatted sql

--changeset repo-admin:CR037 labels:ddl context:all runOnChange:true

-- View: fps.vqryTestsActualBreakdown

DROP VIEW IF EXISTS fps.vqryTestsActualBreakdown;

CREATE VIEW fps.vqryTestsActualBreakdown AS
SELECT DISTINCT
    vtlkpProject_General.Program,
    MonthlyOutput.Buyer,
    tlkpTestCapability.PlanPortfolio AS Portfolio,
    tlkpTestCapability.WorkGroup,
    tlkpTestCapability.TestCode,
    TestOrProduct.ShortDescription,
    MonthlyOutput.Month,
    MonthlyOutput.fpsyear,
    COALESCE(tblTestRCCost.price, tlkpTestReqmt.unitprice) AS PCPrice,
    MonthlyOutput.Volume * COALESCE(tblTestRCCost.price, tlkpTestReqmt.unitprice) AS PCCost,
    CASE
        WHEN MonthlyOutput.Workgroup = 'LTLA' AND tblTestRCCost.profitcentre = 'VetR' THEN 'Path'
        WHEN tblTestRCCost.profitcentre IS NULL THEN vWorkGroup_General.profitcentre
        ELSE tblTestRCCost.profitcentre
    END AS "profitcentre"
FROM fps.tlkpTestReqmt
INNER JOIN fps.tlkpTestCapability
    ON tlkpTestCapability.TestCode = tlkpTestReqmt.TestCode
   AND tlkpTestCapability.fpsyear = tlkpTestReqmt.fpsyear
INNER JOIN fps.MonthlyOutput
    ON tlkpTestCapability.TestCode = MonthlyOutput.TestCode
   AND tlkpTestCapability.WorkGroup = MonthlyOutput.WorkGroup
   AND tlkpTestReqmt.Buyer = MonthlyOutput.Buyer
   AND MonthlyOutput.fpsyear = tlkpTestReqmt.fpsyear
INNER JOIN fps.vWorkGroup_General
    ON MonthlyOutput.WorkGroup = vWorkGroup_General.WorkGroup
   AND vWorkGroup_General.fpsyear = MonthlyOutput.fpsyear
INNER JOIN fps.TestOrProduct
    ON tlkpTestReqmt.TestCode = TestOrProduct.ItemCode
   AND TestOrProduct.fpsyear = tlkpTestReqmt.fpsyear
INNER JOIN fps.vtlkpProject_General
    ON tlkpTestReqmt.Buyer = vtlkpProject_General.ParentProject
   AND vtlkpProject_General.fpsyear = tlkpTestReqmt.fpsyear
LEFT JOIN fps.tblTestRCCost
    ON TestOrProduct.ItemCode = tblTestRCCost.TestCode
   AND tblTestRCCost.fpsyear = tlkpTestReqmt.fpsyear;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vqryTestsActualBreakdown;
