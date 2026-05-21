USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vTimeCostCalcs_AllStaff]
AS
SELECT     WorkGroup, Month, StaffID, Project, GradeCode, Name, Class, Time
FROM         dbo.TimeCostCalcs
UNION ALL
SELECT     dbo.WorkGroupGrade.WorkGroup, dbo.tblPeriod.EndPeriod, dbo.vtblStaff_General.StaffID, '' AS Project, dbo.WorkGroupGrade.GradeCode, 
                      dbo.vtblStaff_General.Name, '' AS Class, 0 AS Time
FROM         dbo.vtblStaff_General INNER JOIN
                      dbo.WorkGroupGrade ON dbo.vtblStaff_General.WorkGroupGrade = dbo.WorkGroupGrade.WGGrade CROSS JOIN
                      dbo.tblPeriod
WHERE     (dbo.tblPeriod.FinalSummariesRun = - 1) AND (dbo.vtblStaff_General.Name NOT LIKE '%general')

GO
