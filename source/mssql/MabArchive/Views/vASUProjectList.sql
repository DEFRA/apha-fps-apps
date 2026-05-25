USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.vASUProjectList
AS
SELECT     dbo.MY_tlkpProject_All.Year, dbo.MY_tlkpProject_All.ParentProject, dbo.MY_tlkpProject_All.Program, dbo.G_tlkpProject.ProjectTitle, 
                      dbo.MY_tlkpProject_All.IsDefraProject
FROM         dbo.MY_tlkpProject_All LEFT OUTER JOIN
                      dbo.G_tlkpProject ON dbo.MY_tlkpProject_All.ParentProject = dbo.G_tlkpProject.ParentProject
WHERE     (MONTH(GETDATE()) IN (1, 2, 3)) AND (dbo.MY_tlkpProject_All.Year >= YEAR(GETDATE()) - 1) OR
                      (MONTH(GETDATE()) IN (4, 5, 6, 7, 8, 9, 10, 11, 12)) AND (dbo.MY_tlkpProject_All.Year >= YEAR(GETDATE()))

GO
