USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view vTimeRecordedRC_Final
as
SELECT
 vTimeRecordedRC.Project, vTimeRecordedRC.ProfitCentre, IsNull(dbo.vPlannedStaffCostsSummary.SumOfplannedhours,0)as SumOfplannedhours, Isnull(dbo.vPlannedStaffCostsSummary.SumOfCost,0) as SumOfCost
FROM vTimeRecordedRC LEFT JOIN dbo.vPlannedStaffCostsSummary ON (vTimeRecordedRC.ProfitCentre = dbo.vPlannedStaffCostsSummary.ProfitCentre) AND (vTimeRecordedRC.Project = dbo.vPlannedStaffCostsSummary.ParentProject)
GROUP BY vTimeRecordedRC.Project, vTimeRecordedRC.ProfitCentre, dbo.vPlannedStaffCostsSummary.SumOfplannedhours, dbo.vPlannedStaffCostsSummary.SumOfCost;

GO
