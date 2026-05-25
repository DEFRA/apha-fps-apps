USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[xxsp_LoadFromFPS]
AS
begin
  set nocount on
  set rowcount 0

  declare @cFPSVersion varchar(10)
  declare @FPSYear int
  declare @RC int
  declare SysCols insensitive scroll cursor
      for (Select FPSName FROM tblFPSYearsToImport)  for read only

  open SysCols
  fetch next from SysCols into @cFPSVersion 
 if @@Fetch_Status <> 0
  begin
    close SysCols
    deallocate SysCols
    return 0
  end

  while @@Fetch_Status = 0
  begin
    set @FPSYear =Cast(Right(@cFPSVersion ,4) as int)
    Exec sp_DeleteYearsFPSData @cFPSVersion, @FPSYear 
    Exec sp_AddYearsFPSData @cFPSVersion, @FPSYear 

    Delete FROM tblFPSYearsToImport WHERE FPSName=@cFPSVersion

    fetch next from SysCols into @cFPSVersion 
  end

  close SysCols
  deallocate SysCols
end

GO
