USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_TimeCostCalcs_non_Current] (@cFPSVersion as VarChar(10), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_TimeCostCalcs(Year,[WorkGroup] ,
	[JobCode] ,
	[Project],
	[Month] ,
	[StaffID] ,
	[GradeCode] ,
	[Name] ,
	[ChargeRate],
	[Class] ,
	[Time],
	[Cost] ,
	[Division] ,
	[JobCodeOld] )
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[WorkGroup] ,
	[JobCode] ,
	[Project],
	[Month] ,
	[StaffID] ,
	[GradeCode] ,
	[Name] ,
	[ChargeRate],
	[Class] ,
	[Time],
	[Cost] ,
	[Division] ,
	[JobCodeOld] 
	 
FROM  '+ @cFPSVersion + '.dbo.TimeCostCalcs'
Exec(@sqlstr)

GO
