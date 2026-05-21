USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.All_Staff_Project    Script Date: 3/4/00 1:48:19 PM ******/
CREATE VIEW [dbo].[vPlannedStaffCostsPAR1] AS
SELECT dbo.vProjectStaffPlan.ParentProject, 
dbo.vProjectStaffPlan.ProgramNo, 
Sum(dbo.vProjectStaffPlan.plannedhours) AS SumOfplannedhours, 
Sum(dbo.vProjectStaffPlan.Cost) AS SumOfCost
FROM dbo.vProjectStaffPlan INNER JOIN dbo.tlkpProject ON dbo.vProjectStaffPlan.ParentProject = dbo.tlkpProject.ParentProject

GROUP BY dbo.vProjectStaffPlan.ParentProject, dbo.vProjectStaffPlan.ProgramNo
GO
