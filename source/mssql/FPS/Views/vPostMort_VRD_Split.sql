USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.vPostMort_VRD_Split
AS
SELECT TOP (100) PERCENT RIGHT(dbo.MonthlyOutput.WorkGroup, 2) AS Location, dbo.MonthlyOutput.Month, 
               CASE dbo.qryTestsPCCostplan_xtab.VetR WHEN 0 THEN 0 WHEN NULL 
               THEN 0 ELSE 1 END * dbo.qryTestsPCCostplan_xtab.LabT * SUM(dbo.MonthlyOutput.Volume) AS LTSplitFee, 
               CASE dbo.qryTestsPCCostplan_xtab.VetR WHEN 0 THEN 0 WHEN NULL THEN 0 ELSE 1 END AS IsPartOfLTFee, dbo.MonthlyOutput.TestCode, 
               SUM(dbo.MonthlyOutput.Volume) AS TotVol, dbo.qryTestsPCCostplan_xtab.LabT AS LTUnitCharge, dbo.qryTestsPCCostplan_xtab.VetR AS SDUnitCharge, 
               dbo.qryTestsPCCostplan_xtab.LabT * SUM(dbo.MonthlyOutput.Volume) AS LTFee, dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume) 
               AS SDFee, dbo.qryTestsPCCostplan_xtab.LabT * SUM(dbo.MonthlyOutput.Volume) + dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume) 
               AS TotalFee, dbo.tlkpTestReqmt.UnitPrice * SUM(dbo.MonthlyOutput.Volume) - (dbo.qryTestsPCCostplan_xtab.LabT * SUM(dbo.MonthlyOutput.Volume) 
               + dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume)) AS [Profit/Loss], dbo.tlkpTestReqmt.UnitPrice
FROM  dbo.MonthlyOutput INNER JOIN
               dbo.tlkpTestCapability ON dbo.MonthlyOutput.TestCode = dbo.tlkpTestCapability.TestCode AND 
               dbo.MonthlyOutput.WorkGroup = dbo.tlkpTestCapability.WorkGroup INNER JOIN
               dbo.qryTestsPCCostplan_xtab ON dbo.MonthlyOutput.TestCode = dbo.qryTestsPCCostplan_xtab.TestCode INNER JOIN
               dbo.tlkpTestReqmt ON dbo.MonthlyOutput.TestCode = dbo.tlkpTestReqmt.TestCode AND dbo.MonthlyOutput.Buyer = dbo.tlkpTestReqmt.Buyer
GROUP BY dbo.qryTestsPCCostplan_xtab.LabT, dbo.qryTestsPCCostplan_xtab.VetR, dbo.MonthlyOutput.WorkGroup, dbo.MonthlyOutput.Month, 
               dbo.MonthlyOutput.TestCode, dbo.tlkpTestCapability.PlanPortfolio, dbo.tlkpTestReqmt.UnitPrice
HAVING (dbo.tlkpTestCapability.PlanPortfolio IN ('TG0100', 'PMPORT1')) AND (dbo.MonthlyOutput.Month <=
                   (SELECT MAX(EndPeriod) AS Month
                    FROM   dbo.tblPeriod
                    WHERE (FinalSummariesRun = - 1)))
ORDER BY Location, dbo.MonthlyOutput.Month, dbo.MonthlyOutput.TestCode

GO
