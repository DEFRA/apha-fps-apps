USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_LogRecreateSummaries] @Month as smallint 
as
	set nocount on
	DECLARE	@Mno varchar(20)

	EXEC	[dbo].[sp_Get_SP_No]
		@Mno = @Mno OUTPUT

	INSERT Into RecreateSummaries_Log(UserID, Period, DateDone)
	Values(@Mno,@Month,getdate())

GO
