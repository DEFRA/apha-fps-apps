USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectReports_PMMilestoneMail]
AS
SELECT     HLink, ProjectManager, MNumber, ParentProject, Email, EditLink,  Year, Disable
FROM         dbo.vProjectReports_PMMail
WHERE     EXISTS
                          (SELECT     Project, Number, Description, DateDue, DateCompleted, DateFormReceived, UnderSDReview, OnTarget, ProjectLeaderComment, 
                                                   CAPSComment, IDType
                            FROM          dbo.tblMilestone
                            WHERE      (dbo.vProjectReports_PMMail.ParentProject = Project) AND (dbo.vProjectReports_PMMail.Year = DATEPART(yy, DateDue)))

GO
