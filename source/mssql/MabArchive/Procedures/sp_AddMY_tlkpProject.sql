USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run from DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_tlkpProject] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(2000)
set @sqlstr ='INSERT INTO  MY_tlkpProject(Year,
	[ParentProject] ,
	[Program] ,
	[Customer] ,
	[Manager],
	[TransferIncome],
	[CustIncome],
	[WIP_EOY] ,
	[WIP_Limit] ,
	[WIP_Current] ,
	[ProjectStatus] ,
	[DateCreated] ,
	[FECcost],
	[Profit] ,
	[Budget_CVL] ,
	[CaseworkSub] ,
	[PVSIncome] ,
	[PlanCaseworkDebit],
	[Disease] ,
	[Contract] ,
	[Finished]  ,
	[Comments] ,
	[CarryOver] ,
	[IsDefraProject] ,
	[CostCentre] ,
	[OracleProjectCode] ,
	[SubAccountCode] ,
	[ProjectGroup] ,
	[IncomeAccountCode]

)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +' as year, 
	[ParentProject] ,
	[Program] ,
	[Customer] ,
	[Manager],
	[TransferIncome],
	[CustIncome],
	[WIP_EOY] ,
	[WIP_Limit] ,
	[WIP_Current] ,
	[ProjectStatus] ,
	[DateCreated] ,
	[FECcost],
	[Profit] ,
	[Budget_CVL] ,
	[CaseworkSub] ,
	[PVSIncome] ,
	[PlanCaseworkDebit],
	[Disease] ,
	[Contract] ,
	[Finished]  ,
	[Comments] ,
	[CarryOver] ,
	[IsDefraProject] ,
	[CostCentre] ,
	[OracleProjectCode] ,
	[SubAccountCode] ,
	[ProjectGroup] ,
	[IncomeAccountCode]
	 
FROM  '+ @cFPSVersion + '.dbo.tlkpProject'

exec (@sqlstr)

GO
