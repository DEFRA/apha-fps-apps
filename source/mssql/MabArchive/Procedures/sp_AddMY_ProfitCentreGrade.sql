USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Amended to run on DEFACPVWPSQL001 via linked server to VLA88
--


CREATE procedure [dbo].[sp_AddMY_ProfitCentreGrade] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_ProfitCentreGrade(
	[Year] ,
	[PCGrade] ,
	[DivisionGrade] ,
	[GradeCode],
	[ProfitCentre] ,
	[ChargeRate] ,
	[DirectRate]  ,
	[PayRate]  ,
	[NPR] ,
	[OHR] 
	)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +' as year, 

	[PCGrade] ,
	[DivisionGrade] ,
	[GradeCode],
	[ProfitCentre] ,
	[ChargeRate] ,
	[DirectRate]  ,
	[PayRate]  ,
	[NPR] ,
	[OHR] 
	 
FROM  '+ @cFPSVersion + '.dbo.ProfitCentreGrade'

exec(@sqlstr)









GO
