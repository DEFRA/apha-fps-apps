USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vPostMort1]
AS
SELECT     dbo.tlkpTestCapability.PlanPortfolio, dbo.MonthlyOutput.TestCode, dbo.TestOrProduct.ShortDescription AS ItemDescription, 
                      SUM(dbo.MonthlyOutput.Volume) AS TotVol, dbo.qryTestsPCCostplan_xtab.LabT AS LTUnitCharge, 
                      dbo.qryTestsPCCostplan_xtab.VetR AS SDUnitCharge, dbo.qryTestsPCCostplan_xtab.LabT * SUM(dbo.MonthlyOutput.Volume) AS LTfee, 
                      dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume) AS SDfee, SUM(dbo.MonthlyOutput.Volume) 
                      + dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume) AS TotalFee, SUM(dbo.vtblTestRequ.TestPrice * dbo.MonthlyOutput.Volume) 
                      AS FeeCharged, SUM(dbo.vtblTestRequ.TestPrice * dbo.MonthlyOutput.Volume) - SUM(dbo.MonthlyOutput.Volume) 
                      + dbo.qryTestsPCCostplan_xtab.VetR * SUM(dbo.MonthlyOutput.Volume) AS [Profit/(Loss)], dbo.MonthlyOutput.WorkGroup
FROM         dbo.vtblTestRequ INNER JOIN
                      dbo.tlkpTestCapability INNER JOIN
                      dbo.MonthlyOutput ON dbo.tlkpTestCapability.WorkGroup = dbo.MonthlyOutput.WorkGroup AND 
                      dbo.tlkpTestCapability.TestCode = dbo.MonthlyOutput.TestCode ON dbo.vtblTestRequ.TestCode = dbo.MonthlyOutput.TestCode AND 
                      dbo.vtblTestRequ.JobCode = dbo.MonthlyOutput.Buyer INNER JOIN
                      dbo.TestOrProduct ON dbo.MonthlyOutput.TestCode = dbo.TestOrProduct.ItemCode LEFT OUTER JOIN
                      dbo.qryTestsPCCostplan_xtab ON dbo.TestOrProduct.ItemCode = dbo.qryTestsPCCostplan_xtab.TestCode
WHERE     (dbo.MonthlyOutput.Month <=
                          (SELECT     MAX(EndPeriod) AS EndPeriod
                            FROM          tblPeriod
                            WHERE      FinalSummariesRun = - 1))
GROUP BY dbo.tlkpTestCapability.PlanPortfolio, dbo.MonthlyOutput.TestCode, dbo.TestOrProduct.ShortDescription, dbo.qryTestsPCCostplan_xtab.LabT, 
                      dbo.qryTestsPCCostplan_xtab.VetR, dbo.MonthlyOutput.WorkGroup
HAVING      (dbo.tlkpTestCapability.PlanPortfolio LIKE 'tg0100')

GO
