USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[DTrig_tlkpProject] ON dbo.tlkpProject
FOR DELETE
AS
INSERT Project_LOG 
	(
	ParentProject,
	ProjectTitle,
	Program,
	Customer,
	Manager,
	TransferIncome,
	CustIncome,
	WIP_EOY,
	WIP_Limit,
	WIP_Current,
	ProjectStatus,
	CostBookNo,
	DateCreated,
	FECcost,
	Profit,
	Budget_CVL,
	DateCosted,
	Disease,
	Contract,
	ProjectParent,
	ShortTitle,
	CaseworkSub,
	PVSIncome,
	PlanCaseworkDebit,
	Finished,
	OwningRC,

	CarryOver,
	CarryOverSeed,
	Date_Time,
	User_ID,
	Insert_Delete
      ,[IsDefraProject]
      ,[CostCentre]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[ProjectGroup]
	,IncomeAccountCode

)
(SELECT
	ParentProject,
	ProjectTitle,
	Program,
	Customer,
	Manager,
	TransferIncome,
	CustIncome,
	WIP_EOY,
	WIP_Limit,
	WIP_Current,
	ProjectStatus,
	CostBookNo,
	DateCreated,
	FECcost,
	Profit,
	Budget_CVL,
	DateCosted,
	Disease,
	Contract,
	ProjectParent,
	ShortTitle,
	CaseworkSub,
	PVSIncome,
	PlanCaseworkDebit,
	Finished,
	OwningRC,

	CarryOver,
	CarryOverSeed,
	GETDATE(),
	SYSTEM_USER,
	'D'
      ,[IsDefraProject]
      ,[CostCentre]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[ProjectGroup]
	,IncomeAccountCode
FROM DELETED)

GO
