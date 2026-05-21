USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vG_tlkpProjectIncome]
AS
SELECT     dbo.vMY_ProjectCustIncome.Project, COALESCE (dbo.G_tlkpProject_RadTrackData.OverallCustIncome, SUM(dbo.vMY_ProjectCustIncome.CustInc)) 
                      AS TotalProjectValue
FROM         dbo.vMY_ProjectCustIncome LEFT OUTER JOIN
                      dbo.G_tlkpProject_RadTrackData ON dbo.vMY_ProjectCustIncome.Project = dbo.G_tlkpProject_RadTrackData.ParentProject
GROUP BY dbo.vMY_ProjectCustIncome.Project, dbo.G_tlkpProject_RadTrackData.OverallCustIncome

GO
