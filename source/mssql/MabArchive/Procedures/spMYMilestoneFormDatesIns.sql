USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jackie Carter
-- Create date: 17/12/08
-- Description:	Insert new record into MY_MilestoneFormDates
-- =============================================
CREATE PROCEDURE [dbo].[spMYMilestoneFormDatesIns]
	@Year smallint,
	@ParentProject varchar(20),
	@Jan dateTime,
	@Feb dateTime,
	@Mar dateTime,
	@Apr dateTime,
	@May dateTime,
	@Jun dateTime,
	@Jul dateTime,
	@Aug dateTime,
	@Sep dateTime,
	@Oct dateTime,
	@Nov dateTime,
	@Dec dateTime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Insert into dbo.MY_MilestoneFormDates
	(Year ,
	 ParentProject ,
	 Jan ,
	 Feb ,
	 Mar ,
	 Apr ,
	 May ,
	 Jun ,
	 Jul ,
	 Aug ,
	 Sep ,
	 Oct ,
	 Nov ,
	 Dec )
	values
	(@Year ,
	 @ParentProject ,
	 @Jan ,
	 @Feb ,
	 @Mar ,
	 @Apr ,
	 @May ,
	 @Jun ,
	 @Jul ,
	 @Aug ,
	 @Sep ,
	 @Oct ,
	 @Nov ,
	 @Dec
	)
END

GO
