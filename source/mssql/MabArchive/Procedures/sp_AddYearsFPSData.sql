USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



--PL 01.10.24
--Updated to work on DEFACPVWPSQL001 via linked server to VLA88
--
--PL 31.10.24
--Refer to FPS data on local server rather than linked VLA88 (see CHG0111627)
--as FPS<year> databases being moved from VLA88 to PSQL001
--
--PL 28.11.24
--Refer to FPS data on VLA88 again (Server name passed in as part of @cFPSVersion param)
--


CREATE procedure [dbo].[sp_AddYearsFPSData] (@cFPSVersion as varchar(20), @vcFPSYear as int)
AS
BEGIN
	Exec sp_AddMY_tlkpProgram @cFPSVersion , @vcFPSYear 
	Exec sp_AddG_tlkpProject @cFPSVersion
	Exec sp_AddMY_tlkpProject @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_FPSYearTotals @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_MonthlyOutput @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_MonthlyTime @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_Proj_Invoice @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_Proj_SubContract @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_ProjectMonthFinal @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblAdditionalCosts @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblAnimalReq @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblContract @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblStaffJob @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_TimeCostCalcs @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tlkpTestReqmt @cFPSVersion , @vcFPSYear 
	Exec sp_addMY_YearDetails @cFPSVersion , @vcFPSYear 
	Exec sp_addMY_WorkGroupGrade @cFPSVersion , @vcFPSYear 
	Exec sp_addMY_ProfitCentreGrade @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblProfitCentre @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_TestOrProduct @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_Staff @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_Workgroup @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tblAnimals @cFPSVersion , @vcFPSYear 
	Exec sp_AddMY_tlkpProject_All @cFPSVersion , @vcFPSYear 
END



GO
