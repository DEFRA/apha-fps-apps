USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ian Galloway
-- Create date: 13-Jan-2009
-- Description:	Logs changes to tblMilestone for non-web access.
-- Data can be updated by Web App or by Access.  The Web App needs to do its own logging as
-- the user name is the SQL account.  	
-- =============================================
CREATE trigger [dbo].[tblMilestone_IUT]
   ON  [dbo].[tblMilestone] 
   AFTER UPDATE, INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF LEFT(SYSTEM_USER,7)='Demeter\' --Non-web users
	BEGIN

	declare @UserNo varchar(20)
	declare @DT  datetime
	declare @UT as char(1)

	Exec sp_Get_SP_No @UserNo OUTPUT
	set @DT=GETDATE()

	IF (Select count(*) 
			from DELETED INNER JOIN tblMilestone ON DELETED.Project=tblMilestone.Project 
			and DELETED.Number=tblMilestone.Number)=0 --Means its an insert
		set @UT='I'
	ELSE
		set @UT='U'
	
    INSERT INTO [dbo].[tblLOGMilestone](
	[Project] ,
	[Number] ,
	[Description],
	[DateDue] ,
	[DateCompleted] ,
	[DateFormReceived] ,
	[UnderSDReview] ,
	[OnTarget] ,
	[ProjectLeaderComment] ,
	[CAPSComment] ,
	[IDType] ,
	[DateChanged] ,
	[ChangedBy] ,
	[UpdateType] )

    SELECT  
	[Project] ,
	[Number] ,
	[Description],
	[DateDue] ,
	[DateCompleted] ,
	[DateFormReceived] ,
	[UnderSDReview] ,
	[OnTarget] ,
	[ProjectLeaderComment] ,
	[CAPSComment] ,
	[IDType] ,
	@DT ,
	@UserNo ,
	@UT 
	FROM INSERTED
	END
END










GO
