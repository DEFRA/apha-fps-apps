USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--PL 01.10.24 Amended to access FPS data on VLA88 from DEFACPVWPSQL001 via linked server
--
--PL 31.10.24
--Refer to FPS data on local server rather than linked VLA88 (see CHG0111627)
--as FPS<year> databases being moved from VLA88 to PSQL001
--
--PL 28.11.24
--Refer to FPS data on VLA88 again (server name passed in as part of @cFPSVersion param)
--

CREATE procedure [dbo].[sp_DeleteYearsFPSData] (@cFPSVersion as varchar(20), @FPSYear as int)
AS
BEGIN
	declare @sqlstr as varchar(800)

	set @sqlstr ='DELETE FROM G_tlkpProject
	WHERE G_tlkpProject.ParentProject 
		IN(SELECT ParentProject 
	FROM '+ @cFPSVersion + '.dbo.tlkpProject)'
	Exec(@sqlstr)
	DELETE from MY_FPSYearTotals WHERE Year=@FPSYear 
	DELETE from MY_MonthlyOutput WHERE Year=@FPSYear 
	DELETE from MY_Monthlytime WHERE Year=@FPSYear 
	DELETE from MY_Proj_invoice WHERE Year=@FPSYear 
	DELETE from MY_Proj_SubContract WHERE Year=@FPSYear 
	DELETE from MY_ProjectMonthFinal WHERE Year=@FPSYear 
	DELETE from MY_tblAdditionalCosts WHERE Year=@FPSYear 
	DELETE from MY_tblAnimalReq WHERE Year=@FPSYear 
	DELETE from MY_tblContract WHERE Year=@FPSYear 
	DELETE from MY_tblStaffJob WHERE Year=@FPSYear 
	DELETE from MY_TimeCostCalcs WHERE Year=@FPSYear 
	DELETE from MY_tlkpTestReqmt WHERE Year=@FPSYear 
	DELETE from MY_tlkpProject WHERE Year=@FPSYear 
	DELETE from MY_tlkpProgram WHERE Year=@FPSYear 
	DELETE from tlkpYear WHERE Year=@FPSYear 

	DELETE from MY_ProfitCentreGrade WHERE Year=@FPSYear 
	DELETE from MY_WorkGroupGrade WHERE Year=@FPSYear 
	DELETE from MY_tblProfitCentre WHERE Year=@FPSYear 
	DELETE from MY_TestOrProduct WHERE Year=@FPSYear 
	DELETE from MY_Staff WHERE Year=@FPSYear 
	DELETE from MY_Workgroup WHERE Year=@FPSYear 
	DELETE from MY_tblAnimals WHERE Year=@FPSYear 
	DELETE from MY_tlkpProject_all WHERE Year=@FPSYear 
END


GO
