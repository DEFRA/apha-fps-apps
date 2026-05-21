USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run on DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_TimeCostCalcs] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_TimeCostCalcs
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
	[JobCodeOld] ,
	[Pay] ,
	[NonPay] ,
	[Overhead]
	 
FROM  '+ @cFPSVersion + '.dbo.TimeCostCalcs'
Exec(@sqlstr)

GO
