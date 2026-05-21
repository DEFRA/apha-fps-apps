USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectLatestDetails]
AS
SELECT     dbo.G_tlkpProject.ParentProject, dbo.MY_tlkpProject.Program, dbo.MY_tlkpProject.Manager, dbo.G_tlkpProject.ProjectTitle, 
                      dbo.G_tlkpProject.ShortTitle, dbo.MY_tlkpProject.Customer, dbo.vLatestProjectYear.Year AS LastYear, CASE WHEN vLatestProjectYear.Year =
                          (SELECT     MAX(Year)
                            FROM          tlkpYear) THEN 'Y' ELSE 'N' END AS Active, dbo.MY_tlkpProject.ProjectGroup
FROM         dbo.G_tlkpProject INNER JOIN
                      dbo.vLatestProjectYear ON dbo.G_tlkpProject.ParentProject = dbo.vLatestProjectYear.ParentProject INNER JOIN
                      dbo.MY_tlkpProject ON dbo.MY_tlkpProject.Year = dbo.vLatestProjectYear.Year AND 
                      dbo.MY_tlkpProject.ParentProject = dbo.vLatestProjectYear.ParentProject


GO
