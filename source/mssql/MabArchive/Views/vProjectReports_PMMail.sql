USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectReports_PMMail]
AS
SELECT     '<a href="' + Root.Setting + '\' + dbo.vCurrent_ProjectInfo.ProjectGroup + '\' + REPLACE(dbo.vCurrent_ProjectInfo.ParentProject, '/', '-') 
                      + ' ' + PRepName.Setting + '" target="_blank">' + dbo.vCurrent_ProjectInfo.ParentProject + '</a><br> ' AS HLink, CASE WHEN (ProjectManager LIKE N'%,%') 
                      THEN SUBSTRING(ProjectManager, PATINDEX('%,%', ProjectManager) + 2, 50) + ' ' + LEFT(ProjectManager, PATINDEX('%,%', ProjectManager) - 1) 
                      ELSE ProjectManager END AS ProjectManager, dbo.tblProjectManager.MNumber, dbo.vCurrent_ProjectInfo.ParentProject, dbo.tblProjectManager.Email, 
                      '<a href="' + EditRoot.Setting + dbo.vCurrent_ProjectInfo.ParentProject + '">' + dbo.vCurrent_ProjectInfo.ParentProject + '</a><br>' AS EditLink, 
                      dbo.vCurrent_ProjectInfo.ProjectGroup, dbo.vCurrent_ProjectInfo.Year, dbo.tblProjectManager.Disable
FROM         dbo.tblRadTrackProg INNER JOIN
                      dbo.tblProjectManager INNER JOIN
                      dbo.vCurrent_ProjectInfo ON dbo.tblProjectManager.ProjectManager = dbo.vCurrent_ProjectInfo.Manager ON 
                      dbo.tblRadTrackProg.Program = dbo.vCurrent_ProjectInfo.Program CROSS JOIN
                      dbo.[tbl Settings] AS PRepName CROSS JOIN
                      dbo.[tbl Settings] AS Root CROSS JOIN
                      dbo.[tbl Settings] AS EditRoot
WHERE     (Root.ID = N'PIMS_Project_Current_Root') AND ([PRepName ].ID = N'PIMS_Project_Report_Name') AND (EditRoot.ID = N'PIMS_Project_Edit_Link') AND 
                      (dbo.tblRadTrackProg.RadTrackProg = 1) AND (dbo.vCurrent_ProjectInfo.ProjectStatus <> 'Completed') AND (dbo.vCurrent_ProjectInfo.ProjectGroup NOT IN ('EU_PROG',
                       'ZT_Prog'))

GO
