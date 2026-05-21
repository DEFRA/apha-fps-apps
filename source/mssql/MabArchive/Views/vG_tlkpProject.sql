USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vG_tlkpProject]
AS
SELECT     dbo.G_tlkpProject.ParentProject, dbo.G_tlkpProject.ProjectTitle, dbo.G_tlkpProject.CostBookNo, dbo.G_tlkpProject.Disease, dbo.G_tlkpProject.Contract, 
                      dbo.G_tlkpProject.ShortTitle, dbo.G_tlkpProject.ProjectStatus, dbo.vCurrent_tlkpProjectRadTrackData.BFBudget AS CurrentBFBudget
FROM         dbo.G_tlkpProject LEFT OUTER JOIN
                      dbo.vCurrent_tlkpProjectRadTrackData ON dbo.G_tlkpProject.ParentProject = dbo.vCurrent_tlkpProjectRadTrackData.Project

GO
