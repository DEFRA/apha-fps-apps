USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.vProgramReports_Mail
AS
SELECT     '<a href="' + Root.Setting + '/' + sq.program + '_' + PRepName.Setting + '">' + sq.program + '</a><br> ' AS HLink, CASE WHEN (ProjectManager LIKE N'%,%') 
                      THEN SUBSTRING(ProjectManager, PATINDEX('%,%', ProjectManager) + 2, 50) + ' ' + LEFT(ProjectManager, PATINDEX('%,%', ProjectManager) - 1) 
                      ELSE ProjectManager END AS ProjectManager, dbo.tblProjectManager.MNumber, dbo.tblProjectManager.Email, sq.program, sq.Year, 
                      dbo.tblProjectManager.Disable
FROM         dbo.tblProgram_Manager_Link INNER JOIN
                      dbo.tblRadTrackProg INNER JOIN
                          (SELECT     Year, CASE WHEN projectgroup = 'SCN_RES' THEN projectgroup ELSE program END AS program
                            FROM          dbo.vCurrent_ProjectInfo
                            WHERE      (ProjectStatus <> 'Completed')) AS sq ON dbo.tblRadTrackProg.Program = sq.program ON 
                      dbo.tblProgram_Manager_Link.Program = dbo.tblRadTrackProg.Program INNER JOIN
                      dbo.tblProjectManager ON dbo.tblProgram_Manager_Link.Manager = dbo.tblProjectManager.ProjectManager CROSS JOIN
                      dbo.[tbl Settings] AS Root CROSS JOIN
                      dbo.[tbl Settings] AS PRepName
WHERE     (Root.ID = N'PIMS_Program_Current_Root') AND ([PRepName ].ID = N'PIMS_Program_Report_Name') AND (dbo.tblRadTrackProg.RadTrackProg = 1)
GROUP BY '<a href="' + Root.Setting + '/' + sq.program + '_' + PRepName.Setting + '">' + sq.program + '</a><br> ', dbo.tblProjectManager.MNumber, 
                      dbo.tblProjectManager.Email, sq.program, sq.Year, dbo.tblProjectManager.ProjectManager, dbo.tblProjectManager.Disable
HAVING      (sq.program <> 'ZT_Prog')

GO
