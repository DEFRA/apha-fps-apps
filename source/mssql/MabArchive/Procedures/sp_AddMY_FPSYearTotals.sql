USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run on DEFACPVWPSQL001 via linked server to VLA88
--


CREATE procedure [dbo].[sp_AddMY_FPSYearTotals] (@cFPSVersion as varchar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_FPSYearTotals
SELECT ' + Cast(@vcFPSYear as varChar(10)) +', 
	[ParentProject] ,
	[Program] ,
	[TotalAdditionalCosts] ,
	[TotalAnimalCosts] ,
	[TotalStaffCosts] ,
	[TotalTestCosts],
	[TotalCosts] ,
	[CustIncome] ,
	[TransferIncome] ,
	[TotalIncome] ,
	[Budget_CVL],
	[RequiredProfit],
	[Manager] ,
	[Customer] ,
	[ProjectStatus] ,
	[PVSIncome] ,
	[PlanCaseworkDebit],
	TotalPayCosts
FROM  '+ @cFPSVersion + '.dbo.FPSYearTotals'
Exec(@sqlstr)




GO
