USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jackie Carter
-- Create date: 19/11/2008
-- Description:	gets the non completed milestone details by the project
-- =============================================
CREATE PROCEDURE [dbo].[spMilestoneGetByProject]
	
	@project varchar(20),
	@fromDate datetime,
	@toDate	datetime
 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT		dbo.tblMilestone.Project, 
				dbo.tblMilestone.Number as milestone, 
				dbo.tblMilestone.Description, 
				dbo.tblMilestone.DateDue, 
				dbo.tblMilestone.DateCompleted, 
				dbo.tblMilestone.UnderSDReview, 
				dbo.tblMilestone.OnTarget, 
				dbo.tblMilestone.ProjectLeaderComment as projectleadersComment,
				dbo.tlkpMilestoneType.MilestoneDeliverable as milestoneType
	FROM        dbo.tblMilestone,
				dbo.tlkpMilestoneType
	WHERE		dbo.tblMilestone.IDType = dbo.tlkpMilestoneType.IDType
	AND			dbo.tblMilestone.Project = @project
	AND			dbo.tblMilestone.DateDue >= @fromDate
	and			dbo.tblMilestone.DateDue <= @toDate
END

GO
