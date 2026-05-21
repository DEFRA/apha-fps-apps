USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qVRD_Split]
AS
SELECT     TOP (100) PERCENT dbo.MonthlyOutput.WorkGroup AS Location, dbo.MonthlyOutput.Month, 
                      (CASE dbo.vPlanCrosstab.VetR WHEN 0 THEN 1 ELSE 0 END) * (dbo.vPlanCrossTab.LabT * SUM(dbo.MonthlyOutput.Volume)) AS LTSplitFee, 
                      CASE dbo.vPlanCrosstab.VetR WHEN 0 THEN 1 ELSE 0 END AS IsPartOfLTFee, dbo.MonthlyOutput.TestCode, SUM(dbo.MonthlyOutput.Volume) 
                      AS TotVol, dbo.vPlanCrossTab.LabT AS LTUnitCharge, dbo.vPlanCrossTab.VetR AS SDUnitCharge, 
                      dbo.vPlanCrossTab.LabT * SUM(dbo.MonthlyOutput.Volume) AS LTFee, dbo.vPlanCrossTab.VetR * SUM(dbo.MonthlyOutput.Volume) AS SDFee, 
                      dbo.vPlanCrossTab.LabT * SUM(dbo.MonthlyOutput.Volume) + dbo.vPlanCrossTab.VetR * SUM(dbo.MonthlyOutput.Volume) AS TotalFee, 
                      dbo.tlkpTestReqmt.UnitPrice * SUM(dbo.MonthlyOutput.Volume) - (dbo.vPlanCrossTab.LabT * SUM(dbo.MonthlyOutput.Volume) 
                      + dbo.vPlanCrossTab.VetR * SUM(dbo.MonthlyOutput.Volume)) AS [Profit/Loss], dbo.tlkpTestReqmt.UnitPrice AS TestPrice
FROM         dbo.MonthlyOutput INNER JOIN
                      dbo.tlkpTestCapability ON dbo.MonthlyOutput.TestCode = dbo.tlkpTestCapability.TestCode AND 
                      dbo.MonthlyOutput.WorkGroup = dbo.tlkpTestCapability.WorkGroup INNER JOIN
                      dbo.vPlanCrossTab ON dbo.MonthlyOutput.TestCode = dbo.vPlanCrossTab.TestCode INNER JOIN
                      dbo.tlkpTestReqmt ON dbo.MonthlyOutput.TestCode = dbo.tlkpTestReqmt.TestCode AND dbo.MonthlyOutput.Buyer = dbo.tlkpTestReqmt.Buyer
GROUP BY dbo.vPlanCrossTab.LabT, dbo.vPlanCrossTab.VetR, dbo.MonthlyOutput.WorkGroup, dbo.MonthlyOutput.Month, dbo.MonthlyOutput.TestCode, 
                      dbo.tlkpTestCapability.PlanPortfolio, dbo.tlkpTestReqmt.UnitPrice
HAVING      (dbo.tlkpTestCapability.PlanPortfolio = 'TG0100') AND (dbo.MonthlyOutput.Month <=
                          (SELECT     MAX(EndPeriod) AS Month
                            FROM          dbo.tblPeriod
                            WHERE      (FinalSummariesRun = - 1))) AND (dbo.vPlanCrossTab.LabT IS NOT NULL) AND (dbo.vPlanCrossTab.VetR IS NOT NULL)
ORDER BY Location, dbo.MonthlyOutput.Month, dbo.MonthlyOutput.TestCode

GO
