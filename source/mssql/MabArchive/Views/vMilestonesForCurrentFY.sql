USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMilestonesForCurrentFY]
AS
SELECT     dbo.tblMilestone.Project, dbo.tblMilestone.Number, dbo.tblMilestone.Description, dbo.tblMilestone.DateDue, dbo.tblMilestone.DateCompleted, 
                      dbo.tblMilestone.DateFormReceived, dbo.tblMilestone.UnderSDReview, dbo.tblMilestone.OnTarget, dbo.tblMilestone.ProjectLeaderComment, 
                      dbo.tblMilestone.CAPSComment, dbo.tblMilestone.IDType, CASE dbo.vLatestMonthYear.Year WHEN DATEPART(YY, DATEADD(MONTH, - 3, 
                      dbo.tblMilestone.DateDue)) THEN - 1 ELSE 0 END AS inThisFYear
FROM         dbo.tblMilestone CROSS JOIN
                      dbo.vLatestMonthYear
WHERE     (DATEPART(YY, DATEADD(MONTH, - 3, dbo.tblMilestone.DateDue)) = dbo.vLatestMonthYear.Year) OR
                      (DATEPART(YY, DATEADD(MONTH, - 6, dbo.tblMilestone.DateDue)) = dbo.vLatestMonthYear.Year)

GO
