USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run from DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_tlkpProgram] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tlkpProgram(Year,
	[ProgramNo] ,
	[ProgramName],
	[Directorate] ,
	[MINIM] ,
	[SECTOR_NAME] ,
	[CUSTOMER] ,
	[Target] ,
	[Manager] 
	)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +' as year, 
	[ProgramNo] ,
	[ProgramName],
	[Directorate] ,
	[MINIM] ,
	[SECTOR_NAME] ,
	[CUSTOMER] ,
	[Target] ,
	[Manager] 
	 
FROM  '+ @cFPSVersion + '.dbo.tlkpProgram'

exec(@sqlstr)

GO
