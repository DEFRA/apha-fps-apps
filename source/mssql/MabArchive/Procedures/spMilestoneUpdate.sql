USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jackie Carter
-- Create date: 01/12/2008
-- Description:	Updates projectMilestones
-- Updated by IG 14/01/2008 to add logging.
-- =============================================
CREATE PROCEDURE [dbo].[spMilestoneUpdate]
	@project varchar(20),
	@Milestone varchar(10),
	@UnderReview smallint,
	@OnTarget smallint,
	@DateCompleted dateTime,
	@projectLeaderComments varchar(max),
	@UserID varchar(10)
AS
BEGIN
	declare @Description varchar(500)
	declare @DateDue datetime
	declare @CAPSComment varchar(250) 
	declare @IDType char(1)

	SET NOCOUNT ON;
	BEGIN TRAN
		SELECT @Description=[Description], 
			@DateDue=[DateDue],
			@CAPSComment=[CAPSComment],
			@IDType=[IDType]
		FROM tblMilestone
		WHERE Project=@project and Number=@Milestone

	

	    INSERT INTO [dbo].[tblLOGMilestone](
		[Project] ,
		[Number] ,
		[Description],
		[DateDue] ,
		[DateCompleted] ,
		[UnderSDReview] ,
		[OnTarget] ,
		[ProjectLeaderComment] ,
		[CAPSComment] ,
		[IDType] ,
		[DateChanged] ,
		[ChangedBy] ,
		[UpdateType] )

	    VALUES  (
		@project ,
		@Milestone ,
		@Description,
		@DateDue ,
		@DateCompleted ,
		@UnderReview ,
		@OnTarget ,
		@projectLeaderComments ,
		@CAPSComment ,
		@IDType ,
		Getdate() ,
		@UserID ,
		'U' )
	
	IF (@@error <> 0) 
		ROLLBACK TRAN
	ELSE
	BEGIN
	    Update tblMilestone
		set UnderSDReview = @UnderReview,
			OnTarget = @OnTarget,
			DateCompleted = @DateCompleted,
			projectLeaderComment = @ProjectLeaderComments 
		Where project = @project
		and		Number = @milestone
	
		COMMIT TRAN
		
	END
END

GO
