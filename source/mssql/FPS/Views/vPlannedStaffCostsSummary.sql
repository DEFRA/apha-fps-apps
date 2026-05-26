USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vPlannedStaffCostsSummary]
AS
SELECT dbo.WorkGroup.ProfitCentre, dbo.vProjectStaffPlan.ParentProject, SUM(dbo.vProjectStaffPlan.Cost) AS SumOfCost, SUM(dbo.vProjectStaffPlan.plannedhours) 
                  AS SumOfplannedhours
FROM     dbo.vProjectStaffPlan INNER JOIN
                  dbo.WorkGroup ON dbo.vProjectStaffPlan.WorkGroup = dbo.WorkGroup.WorkGroup
GROUP BY dbo.WorkGroup.ProfitCentre, dbo.vProjectStaffPlan.ParentProject


GO
