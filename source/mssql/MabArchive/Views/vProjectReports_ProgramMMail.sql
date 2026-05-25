USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectReports_ProgramMMail]
AS
SELECT     '<a href="' + Root.Setting + '/' + sq.Program + '_' + PRepName.Setting + '">' + sq.Program + '</a><br> ' AS HLink, 
                      CASE WHEN (ProjectManager LIKE N'%,%') THEN SUBSTRING(ProjectManager, PATINDEX('%,%', ProjectManager) + 2, 50) + ' ' + LEFT(ProjectManager, 
                      PATINDEX('%,%', ProjectManager) - 1) ELSE ProjectManager END AS ProjectManager, dbo.tblProjectManager.MNumber, dbo.tblProjectManager.Email, 
                      sq.Program, sq.Year, dbo.tblProjectManager.Disable
FROM         dbo.tblProgram_Manager_Link INNER JOIN
                      dbo.tblRadTrackProg INNER JOIN
                       (select year, case when projectgroup ='SCN_RES' then projectgroup else program end as program from dbo.vCurrent_ProjectInfo
where ProjectStatus<>'Completed') as sq ON dbo.tblRadTrackProg.Program = sq.Program ON 
                      dbo.tblProgram_Manager_Link.Program = dbo.tblRadTrackProg.Program INNER JOIN
                      dbo.tblProjectManager ON dbo.tblProgram_Manager_Link.Manager = dbo.tblProjectManager.ProjectManager CROSS JOIN
                      dbo.[tbl Settings] AS Root CROSS JOIN
                      dbo.[tbl Settings] AS PRepName
WHERE     ([PRepName ].ID = N'PIMS_Program_Report_Name') AND (dbo.tblRadTrackProg.RadTrackProg = 1) AND  
                      (Root.ID = N'PIMS_Program_Current_Root')
GROUP BY '<a href="' + Root.Setting + '/' + sq.Program + '_' + PRepName.Setting + '">' + sq.Program + '</a><br> ', 
                      dbo.tblProjectManager.MNumber, dbo.tblProjectManager.Email, sq.Program, sq.Year, 
                      dbo.tblProjectManager.ProjectManager, dbo.tblProjectManager.Disable
HAVING      (sq.Program <> 'ZT_Prog')
GO
