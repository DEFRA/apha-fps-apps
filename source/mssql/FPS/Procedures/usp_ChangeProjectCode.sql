USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Stored Procedure dbo.usp_ChangeProjectCode    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_ChangeProjectCode    Script Date: 7/22/99 12:07:57 PM ******/
CREATE PROCEDURE [dbo].[usp_ChangeProjectCode] 
	@OldCode Varchar(20),
	@NewCode VarChar(20)
AS
BEGIN TRANSACTION
IF (SELECT COUNT(*) FROM tlkpProject WHERE tlkpProject.parentProject = @NewCode ) != 0 
	BEGIN
		RAISERROR ( 'This code is already in use. ',10,1)
		ROLLBACK TRANSACTION
	END
INSERT INTO tlkpProject 
	( ParentProject, 
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
	owningRC,
	Comments,
	CarryOver,
	CarryOverSeed ,
	IsDefraProject,
	CostCentre
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[ProjectGroup]
      ,[IncomeAccountCode]
)
SELECT @NewCode, 
	tlkpProject.ProjectTitle, 
	tlkpProject.Program, 
	tlkpProject.Customer, 
	tlkpProject.Manager, 
	tlkpProject.TransferIncome, 
	tlkpProject.CustIncome, 
	tlkpProject.WIP_EOY, 
	tlkpProject.WIP_Limit, 
	tlkpProject.WIP_Current, 
	tlkpProject.ProjectStatus, 
	tlkpProject.CostBookNo, 
	tlkpProject.FECcost, 
	tlkpProject.Profit,
	tlkpProject.Budget_CVL, 
	tlkpProject.DateCosted, 
	tlkpProject.Disease, 
	tlkpProject.Contract, 
	tlkpProject.ProjectParent,
	tlkpProject.ShortTitle,
	tlkpProject.CaseworkSub,
	tlkpProject.PVSIncome,
	tlkpProject.PlanCaseworkDebit,
	tlkpProject.Finished,
	tlkpProject.owningRC,
	tlkpProject.Comments,
	tlkpProject.CarryOver,
	tlkpProject.CarryOverSeed ,
	tlkpProject.IsDefraProject,
	tlkpProject.CostCentre
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[ProjectGroup]
      ,[IncomeAccountCode]
FROM tlkpProject
WHERE tlkpProject.ParentProject = @OldCode

INSERT INTO tlkpJobcode(JobCode,parentProject,JobCodeWorkgroup,NewProg,Type,JobcodeName)
SELECT 
	CASE jc.JobCode
		WHEN @OldCode THEN @NewCode
			ELSE jc.JobCode
		END,
	@NewCode, JobCodeWorkgroup,NewProg,Type,JobcodeName

FROM tlkpJobCode jc
WHERE jc.Parentproject = @OldCode

UPDATE tc
SET tc.PlanPortfolio = @NewCode
FROM tlkpTestCapability tc
WHERE tc.PlanPortfolio = @OldCode
EXECUTE sp_insert_tcv @oldcode, @newcode
EXECUTE sp_insert_tr @oldcode, @newcode
UPDATE mt
SET mt.Parentproject = @NewCode,
	mt.TimeCode = CASE mt.TimeCode
			WHEN  @oldCode THEN @NewCode
			ELSE mt.TimeCode
			END
FROM MonthlyTime mt
WHERE mt.Parentproject = @OldCode
UPDATE mo
SET mo.Buyer = @NewCode
FROM MonthlyOutput mo
WHERE mo.Buyer = @OldCode
UPDATE ac
SET ac.JobCode = @NewCode
FROM tblAdditionalCosts ac
WHERE ac.JobCode = @OldCode
UPDATE pi
SET pi.ProjectParent = @NewCode
FROM Proj_Invoice pi
WHERE pi.ProjectParent = @OldCode
UPDATE psc
SET psc.Project = @NewCode
FROM Proj_SubContract psc
WHERE psc.Project = @OldCode
UPDATE tcc
SET tcc.Project = @NewCode,
	tcc.JobCode = CASE tcc.JobCode
			WHEN  @oldCode THEN @NewCode
			ELSE tcc.JobCode
			END
FROM TimeCostCalcs tcc
WHERE tcc.Project = @OldCode
UPDATE pm
SET pm.Project = @NewCode
FROM ProjectMonth pm
WHERE pm.Project = @OldCode
UPDATE ar
SET ar.JobCode = @NewCode
FROM tblAnimalReq ar
WHERE ar.JobCode = @OldCode
UPDATE ms
SET ms.Project = @NewCode
FROM Milestone ms
WHERE ms.Project = @OldCode
UPDATE sj
SET sj.Jobcode = @NewCode
FROM tblStaffJob sj
WHERE sj.Jobcode = @OldCode
UPDATE pmf
SET pmf.Project = @NewCode
FROM ProjectMonthFinal pmf
WHERE pmf.Project = @OldCode
EXECUTE sp_Delete_tr @oldcode
EXECUTE sp_Delete_tcv @oldcode
EXECUTE sp_Delete_jc @oldcode
EXECUTE sp_Delete_pp @oldcode

COMMIT TRANSACTION


GO
