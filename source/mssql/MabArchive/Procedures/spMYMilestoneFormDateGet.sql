USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jackie Carter
-- Create date: 18/12/2008
-- Description:	Gets the MilestoneFormDates 
-- =============================================
CREATE PROCEDURE [dbo].[spMYMilestoneFormDateGet]
	@Project varchar(20),
	@Year smallint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Select	parentProject as Project ,
			Year as ProjectYear ,
			Jan ,
			Feb ,
			Mar ,
			Apr ,
			May ,
			Jun ,
			Jul ,
			Aug ,
			Sep ,
			Aug ,
			Sep ,
			Oct ,
			Nov ,
			Dec
	From	dbo.MY_MilestoneFormDates
	Where	parentProject = @Project
	and		Year = @Year

END

GO
