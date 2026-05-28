USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



create VIEW [dbo].[vRCReports_Mail]
AS
SELECT     dbo.MY_tblProfitCentre.ProfitCentre, '<a href="' + Root.Setting + '/'  + dbo.MY_tblProfitCentre.ProfitCentre
                      + ' ' + RCRepName1.Setting + '">' + dbo.MY_tblProfitCentre.ProfitCentre
                      + ' ' + RCRepName1.Setting + '</a><br> ' AS HLink1, 
			'<a href="' + Root.Setting + '/'  + dbo.MY_tblProfitCentre.ProfitCentre
                      + ' ' + RCRepName2.Setting + '">' + dbo.MY_tblProfitCentre.ProfitCentre
                      + ' ' + RCRepName2.Setting + '</a><br> ' AS HLink2, 
					CASE WHEN (ProjectManager LIKE N'%,%') 
                      THEN SUBSTRING(ProjectManager, PATINDEX('%,%', ProjectManager) + 2, 50) + ' ' + LEFT(ProjectManager, PATINDEX('%,%', ProjectManager) - 1) 
                      ELSE ProjectManager END AS ProjectManager, dbo.tblProjectManager.MNumber, dbo.tblProjectManager.Email, 
                       dbo.tblProjectManager.Disable
FROM         dbo.vLatestMonthYear INNER JOIN
                      dbo.MY_tblProfitCentre ON dbo.vLatestMonthYear.Year = dbo.MY_tblProfitCentre.Year INNER JOIN
                      dbo.tblProfitCentre_Manager_Link ON dbo.MY_tblProfitCentre.ProfitCentre = dbo.tblProfitCentre_Manager_Link.ProfitCentre INNER JOIN
                      dbo.tblProjectManager ON dbo.tblProfitCentre_Manager_Link.Manager = dbo.tblProjectManager.ProjectManager CROSS JOIN
                      dbo.[tbl Settings] AS RCRepName1 CROSS JOIN
						dbo.[tbl Settings] AS RCRepName2 CROSS JOIN
                      dbo.[tbl Settings] AS Root 
WHERE     (Root.ID = N'PIMS_RC_Current_Root') AND ([RCRepName1].ID = N'PIMS_RC_Report_Name1') 
AND ([RCRepName2].ID = N'PIMS_RC_Report_Name2') 

GO
