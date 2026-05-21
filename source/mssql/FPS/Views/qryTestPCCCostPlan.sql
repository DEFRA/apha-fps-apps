USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryTestPCCCostPlan]
AS
SELECT     TOP (100) PERCENT dbo.vtblTestRequ_TM.JobCode, dbo.vtblTestRequ_TM.TestCode, dbo.tblTestRCCost.ProfitCentre, 
                      CAST(dbo.tblTestRCCost.Price AS float) AS Price
FROM         dbo.vtblTestRequ_TM LEFT OUTER JOIN
                      dbo.tblTestRCCost ON dbo.vtblTestRequ_TM.TestCode = dbo.tblTestRCCost.TestCode
WHERE     (dbo.tblTestRCCost.Price IS NOT NULL) AND (dbo.tblTestRCCost.ProfitCentre IS NOT NULL)
ORDER BY dbo.vtblTestRequ_TM.JobCode

GO
