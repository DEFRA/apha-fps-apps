USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24 
--Updated to run from DEFACPVWPSQL001 via linked server to VLA88
--


CREATE procedure [dbo].[sp_AddMY_Proj_SubContract] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_Proj_SubContract

           ([Year]
           ,[SubContCounter]
           ,[Project]
           ,[TestJob]
           ,[Month]
           ,[Amount]
           ,[WorkGroup]
           ,[AcctCode]
           ,[Supplier]
           ,[Description]
           ,[SupplierNumber]
           ,[DailyRate]
           ,[AnimalDays])

SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	
	[SubContCounter] ,
	[Project] ,
	[TestJob] ,
	[Month] ,
	[Amount],
	[WorkGroup] ,
	[AcctCode] ,
	[Supplier] ,
	[Description] ,
	[SupplierNumber] ,
	[DailyRate] ,
	[AnimalDays] 

FROM  '+ @cFPSVersion + '.dbo.Proj_SubContract'
Exec(@sqlstr)


GO
