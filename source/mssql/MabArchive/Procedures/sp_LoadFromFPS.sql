USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[sp_LoadFromFPS] AS
--Loads data into the MAB Archive from this calendar  year and last, to cater for the financial year.
--(First it deletes and reloads the FPSTotals tables in the years FPS.
--
--PL 01.10.24
--Amended to access FPS databases via linked server to VLA88 (from DEFACPVWPSQL001)
--
--PL 31.10.24
--Pull FPS data from local server rather than linked VLA88 (see CHG0111627)
--as FPS<year> databases being moved from VLA88 to PSQL001
--
--PL 28.11.24
--FPS<year> databases moved back to VLA88, so access via linked server
--All server name shenanigans sorted in THIS proc and passed in as part of
--the parameters passed in to the lower-level procs.
--
--PL 03.04.25
--MAB_Archive copied on to TSQL003 from PSQL001, so this proc needed to be updated to 
--pull data from VLA88Test not VLA88
--
--PL 16.05.25
--FPSyyyy databases have been moved from VLA88Test to DEFACPVWTSQL003, so updated proc to 
--look locally rather than via linked server.
--

  declare @cFPSVersion varchar(20)
  declare @FPSYear int
  declare @RC int
  Declare @sql VARCHAR(50)


SET @FPSYear=DatePart(yyyy,Getdate())-1 
SET @cFPSVersion='FPS'+cast(@FPSYear as varchar(4))
--if (SELECT Count(*) FROM VLA88Test.master.dbo.sysdatabases where name =@cFPSVersion)>0
if (SELECT Count(*) FROM master.dbo.sysdatabases where name =@cFPSVersion)>0
  begin
	--SET @cFPSVersion = 'VLA88Test.' + @cFPSVersion
	set @sql = @cFPSVersion +'.dbo.sp_deleteFPSTotals'
	Exec  @sql
	set @sql = @cFPSVersion +'.dbo.sp_createFPSTotals'
	Exec  @sql    
	Exec sp_DeleteYearsFPSData @cFPSVersion, @FPSYear 
	Exec sp_AddYearsFPSData @cFPSVersion, @FPSYear 
  end

if DATEPART(month, GETDATE()) >4
begin
	SET @FPSYear=DatePart(yyyy,Getdate())
	SET @cFPSVersion='FPS'+cast(@FPSYear as varchar(4))
--	if (SELECT Count(*) FROM VLA88Test.master.dbo.sysdatabases where name =@cFPSVersion)>0
	if (SELECT Count(*) FROM master.dbo.sysdatabases where name =@cFPSVersion)>0
	begin
		--SET @cFPSVersion = 'VLA88Test.' + @cFPSVersion
		set @sql = @cFPSVersion +'.dbo.sp_deleteFPSTotals'
		Exec  @sql
		set @sql = @cFPSVersion +'.dbo.sp_createFPSTotals'
		Exec  @sql   
	    
		Exec sp_DeleteYearsFPSData @cFPSVersion, @FPSYear 
		Exec sp_AddYearsFPSData @cFPSVersion, @FPSYear 
	end
end
else
  begin
	  SET @FPSYear=DatePart(yyyy,Getdate())
	  SET @cFPSVersion='FPS'+cast(@FPSYear as varchar(4))
--	  if (SELECT Count(*) FROM VLA88Test.Master.dbo.sysdatabases where name =@cFPSVersion)>0
	  if (SELECT Count(*) FROM Master.dbo.sysdatabases where name =@cFPSVersion)>0
	  begin  
		DELETE from MY_tlkpProject_all WHERE Year=@FPSYear 
		--SET @cFPSVersion = 'VLA88Test.' + @cFPSVersion
		Exec sp_AddMY_tlkpProject_All @cFPSVersion, @FPSYear 
	  end
  end


GO
