USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Stored Procedure dbo.sp_RecreateSummaries    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.sp_RecreateSummaries    Script Date: 1/12/99 12:14:26 PM ******/
CREATE PROC [dbo].[sp_RecreateSummaries] @Month int AS
declare @periodLocked as smallint

EXECUTE sp_deleteFPSTotals
EXECUTE sp_createFPSTotals
EXECUTE sp_InsertMissingProjects
EXECUTE sp_deleteTimeCostCalcs 
EXECUTE sp_CreateTimeCostCalcs
EXECUTE sp_DeleteProjectMonthCasework
EXECUTE sp_CreateProjectMonthCasework
EXECUTE sp_DeleteProjectMonthFinal 
EXECUTE sp_deleteProjectMonth2 
EXECUTE sp_qryJobMonth_Single
EXECUTE sp_DeleteProjectMonth3
EXECUTE sp_qryJobMonthCum 
EXECUTE sp_qryJobMonth_Final @Month
EXECUTE usp_LogRecreateSummaries @Month
select @periodLocked=periodLocked
FROM         tblPeriod
where endperiod=@month

if @periodLocked=0
begin
	EXECUTE usp_Refresh_Period_MO @month
	EXECUTE usp_Refresh_Period_psc @month
	EXECUTE usp_Refresh_Period_tcc @month
end

GO
