USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jackie Carter
-- Create date: 17/12/08
-- Description:	Updates MY_MilestoneFormDates record
-- =============================================
CREATE PROCEDURE [dbo].[spMYMilestoneFormDatesUpd]
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
	update dbo.MY_MilestoneFormDates
	Set Jan	= @Jan ,
		Feb = @Feb ,
		Mar	= @Mar ,
		Apr = @Apr ,
		May = @May ,
		Jun = @Jun ,
		Jul = @Jul ,
		Aug = @Aug ,
		Sep = @Sep ,
		Oct = @Oct ,
		Nov = @Nov ,
		Dec = @Dec
	where ParentProject = @ParentProject
	and   Year = @Year
	
END

GO
