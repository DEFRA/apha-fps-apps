/*
  Consolidated child/conditional procedure SQL for sp_RecreateSummaries
  Order follows user-requested sequence.
*/

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_deleteFPSTotals    Script Date: 3/4/00 1:48:21 PM ******/
CREATE procEDURE [dbo].[sp_deleteFPSTotals] AS
DELETE from FPSYearTotals

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_createFPSTotals] AS
INSERT INTO FPSYearTotals
SELECT DISTINCT 
	tlkpProject.ParentProject,
	tlkpProject.Program, 
	
	CASE
		WHEN qryTotalAdditionalCosts.TotalAdditionalCosts IS NULL THEN
			0
		ELSE
			qryTotalAdditionalCosts.TotalAdditionalCosts
		END AS TotalAdditionalCosts, 
	CASE
		WHEN qryTotalAnimalCosts.TotalAnimalCosts IS NULL THEN
			0
		ELSE
			qryTotalAnimalCosts.TotalAnimalCosts
		END AS TotalAnimalCosts, 
	CASE
		WHEN qryTotalStaffCosts.TotalStaffCosts IS NULL THEN
			0
		ELSE
			qryTotalStaffCosts.TotalStaffCosts
		END AS TotalStaffCosts, 
	CASE
		WHEN qryTotalTestCosts.TotalTestCosts IS NULL THEN
			0
		ELSE
			qryTotalTestCosts.TotalTestCosts
		END AS TotalTestCosts, 
	CASE
		WHEN qryTotalAdditionalCosts.TotalAdditionalCosts IS NULL THEN
			0
		ELSE
			qryTotalAdditionalCosts.TotalAdditionalCosts
		END + 
	CASE
		WHEN qryTotalAnimalCosts.TotalAnimalCosts IS NULL THEN
			0
		ELSE
			qryTotalAnimalCosts.TotalAnimalCosts
		END  + 
	CASE
		WHEN qryTotalStaffCosts.TotalStaffCosts IS NULL THEN
			0
		ELSE
			qryTotalStaffCosts.TotalStaffCosts
		END +
	CASE
		WHEN qryTotalTestCosts.TotalTestCosts IS NULL THEN
			0
		ELSE
			qryTotalTestCosts.TotalTestCosts
		END +
CASE
		WHEN tlkpProject.PlanCaseworkDebit  IS NULL THEN
			0
		ELSE
 			tlkpProject.PlanCaseworkDebit 
		END AS TotalCosts,
	tlkpProject.CustIncome, 
	tlkpProject.TransferIncome, 
	custincome+Transferincome AS TotalIncome, 
	tlkpProject.Budget_CVL,
	tlkpProject.Profit as RequiredProfit, 
	tlkpProject.Manager, 
	tlkpProject.Customer, 
	tlkpProject.ProjectStatus, 
	Case 
		WHEN tlkpProject.PVSIncome  IS NULL THEN
			0
		ELSE
			tlkpProject.PVSIncome 
		END AS PVSIncome,
	CASE
		WHEN tlkpProject.PlanCaseworkDebit  IS NULL THEN
			0
		ELSE
 			tlkpProject.PlanCaseworkDebit 
		END AS PlanCaseworkDebit,

	CASE
		WHEN qryTotalStaffCosts.TotalPayCosts IS NULL THEN
			0
		ELSE
			qryTotalStaffCosts.TotalPayCosts
		END AS TotalPayCosts

FROM (((tlkpProject 
LEFT JOIN qryTotalAdditionalCosts ON tlkpProject.ParentProject = qryTotalAdditionalCosts.JobCode) 
LEFT JOIN qryTotalAnimalCosts ON tlkpProject.ParentProject = qryTotalAnimalCosts.JobCode) 
LEFT JOIN qryTotalStaffCosts ON tlkpProject.ParentProject = qryTotalStaffCosts.Jobcode) 
LEFT JOIN qryTotalTestCosts ON tlkpProject.ParentProject = qryTotalTestCosts.JobCode

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_InsertMissingProjects] AS
DECLARE @Month int
DECLARE @Message varchar(10)
SELECT @month = 1
WHILE (@month < 13) 
BEGIN
	INSERT INTO ProjectMonth (Project, MonthNo)
	SELECT DISTINCT tlkpProject.ParentProject,
			@month AS MonthNo
	FROM 		tlkpProject LEFT JOIN ProjectMonth ON 
			tlkpProject.ParentProject = ProjectMonth.Project
			AND @month = ProjectMonth.MonthNo 
	WHERE ((ProjectMonth.Project IS NULL))
	ORDER BY ParentProject
	SELECT @month = @month + 1
	IF @month = 13
		BREAK
	ELSE
		CONTINUE
END

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_deleteTimeCostCalcs    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_deleteTimeCostCalcs    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[sp_deleteTimeCostCalcs] AS
DELETE FROM timecostcalcs

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_CreateTimeCostCalcs] AS
/*Based on Create-TimeCostCals from Access FE*/
INSERT 	TimeCostCalcs 	(WorkGroup,
			JobCode,
			Project, 
			Month, 
			StaffID, 
			GradeCode, 
			Name,
			ChargeRate, 
			Class, 
			Time, 
			Cost, 
			Division,
			Pay,
			NonPay,
			OverHead)

SELECT DISTINCT 	WorkGroupGrade.WorkGroup,
			MonthlyTime.TimeCode AS JobCode, 
			TimeCodeValid.ParentProject AS Project, 
			MonthlyTime.Month, 
			vPacttblStaff.PACTid AS StaffID, 
			WorkGroupGrade.GradeCode, 
			vPacttblStaff.Name, 
			case tlkpProject.isdefraproject
				when 0 then
					ProfitCentreGrade.ChargeRate
				else
					ProfitCentreGrade.DefraChargeRate
			end as chargerate,
			CASE 
				WHEN tlkpProgram.sector_name='Charge' THEN
					'Charge'
				ELSE
					'Free'
			END AS Class,
			MonthlyTime.Hours AS Time, 
			CASE 
				WHEN tlkpProgram.sector_name='Charge' THEN
					hours  
				ELSE
					0
			END *
			case tlkpProject.isdefraproject
				when 0 then
					ProfitCentreGrade.ChargeRate
				else
					ProfitCentreGrade.DefraChargeRate
			end AS Cost, 
			tblkpProfitCentre.Division,
			MonthlyTime.Hours * ProfitCentreGrade.PayRate AS Pay,
			MonthlyTime.Hours * ProfitCentreGrade.NPR  AS NonPay,
			MonthlyTime.Hours *ProfitCentreGrade.OHR AS OverHead

FROM (((tblkpProfitCentre 
	INNER JOIN ProfitCentreGrade ON tblkpProfitCentre.ProfitCentre = ProfitCentreGrade.ProfitCentre)
	INNER JOIN WorkGroupGrade ON ProfitCentreGrade.PCGrade = WorkGroupGrade.ProfitCentreGrade) 
	INNER JOIN (TimeCodeValid 
	INNER JOIN (vPACTtblStaff 
	INNER JOIN MonthlyTime ON VPACTtblStaff.PACTid = MonthlyTime.PactStaffID) 
		ON (TimeCodeValid.WorkGroup = MonthlyTime.WorkGroup) 
		AND (TimeCodeValid.TimeCode = MonthlyTime.TimeCode) 
		AND (TimeCodeValid.ParentProject = MonthlyTime.ParentProject)) 
		ON WorkGroupGrade.WGGrade = vPACTtblStaff.WorkGroupGrade) 
	INNER JOIN tlkpProject ON TimeCodeValid.ParentProject = tlkpProject.ParentProject
INNER JOIN tlkpProgram ON tlkpProgram.ProgramNo = tlkpProject.Program

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_DeleteProjectMonthCasework    Script Date: 3/4/00 1:48:21 PM ******/
CREATE procEDURE [dbo].[sp_DeleteProjectMonthCasework] AS
DELETE FROM ProjectMonthCasework

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_CreateProjectMonthCasework    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_CreateProjectMonthCasework    Script Date: 6/24/99 4:11:48 PM ******/
CREATE procEDURE [dbo].[sp_CreateProjectMonthCasework] AS
INSERT ProjectMonthCasework
SELECT DISTINCT qryProjectMonthCW.Project, 
	qryProjectMonthCW.MonthNo,
	qryProjectMonthCW.CWDebit,
	qryProjectMonthCW.CWCredit  

FROM qryProjectMonthCW

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_DeleteProjectMonthFinal    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_DeleteProjectMonthFinal    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[sp_DeleteProjectMonthFinal] AS
DELETE FROM ProjectMonthFinal

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_deleteProjectMonth2    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_deleteProjectMonth2    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[sp_deleteProjectMonth2] AS
DELETE FROM ProjectMonth2

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_qryJobMonth_Single] AS 

INSERT INTO 	ProjectMonth2 
		( Project, 
		MonthNo, 
		CostProfile, 
		Subcontracts, 
		Animals, 
		NonAnimal, 
		TimeCosts, 
		TransferCosts, 
		TotalCost, 
		Invoices, 
		COIW, 
		SumOfCostProfile, 
		PortSales, 
		MstoneDue, 
		Due__Done, 
		OnTime,
		TotalHours,
		PayCosts 

		 )
SELECT 	ProjectMonth.Project, 
	ProjectMonth.MonthNo, 
	ProjectMonth.CostProfile, 
	CASE
		WHEN Total IS NULL THEN 0
		ELSE Total
	END AS Subcontracts, 
	CASE
		WHEN Animals IS NULL THEN 0
		ELSE Animals
	END AS Animals,
	CASE
		WHEN Other IS NULL THEN 0
		ELSE Other
	END AS NonAnimal, 
	CASE
		WHEN sumofcost IS NULL THEN 0
		ELSE sumofcost
	END AS TimeCosts, 
	CASE
		WHEN SumOfTransferCost IS NULL THEN 0
		ELSE sumoftransfercost
	END AS TransferCosts, 
	(ISNULL(Total,0) + ISNULL(SumOfCost,0) + ISNULL(sumoftransfercost, 0)) AS TotalCost,
	CASE
		WHEN SumOfAmount1 IS NULL THEN 0
		ELSE sumofamount1
	END AS Invoices, 
	CASE
		WHEN WorkCost IS NULL THEN 0
		ELSE workcost
	END AS COIW,
	qryJobMonth_TotProfile.SumOfCostProfile, 
	CASE
		WHEN Fee IS NULL THEN 0
		ELSE fee
	END AS PortSales,
	qryJobMonthMilestone.MstoneDue, 
	qryJobMonthMilestone.Due__Done, 
	qryJobMonthMilestone.OnTime,
	CASE
		WHEN sumofHours IS NULL THEN 0
		ELSE sumofhours
	END AS TotalHours,
	CASE
		WHEN SumOfPayRate IS NULL THEN 0
		ELSE SumOfPayRate
	END AS SumOfPayRate
FROM 
	((((((ProjectMonth 
	LEFT JOIN qryJobMonth_SubContracts ON 
		(ProjectMonth.MonthNo = qryJobMonth_SubContracts.Month) AND 
		(ProjectMonth.Project = qryJobMonth_SubContracts.Project)) 
	LEFT JOIN qryJobMonth_Time ON 
		(ProjectMonth.MonthNo = qryJobMonth_Time.Month) AND 
		(ProjectMonth.Project = qryJobMonth_Time.Project)) 
	LEFT JOIN qryJobMonthMilestone ON 
		(ProjectMonth.MonthNo = qryJobMonthMilestone.DueMonth) AND 
		(ProjectMonth.Project = qryJobMonthMilestone.Project)) 
	LEFT JOIN qryJobMonth_TransfersTotal ON 
		(ProjectMonth.MonthNo = qryJobMonth_TransfersTotal.Month) AND 
		(ProjectMonth.Project = qryJobMonth_TransfersTotal.Project)) 
	LEFT JOIN qryJobMonth_Invoices ON 
		(ProjectMonth.MonthNo = qryJobMonth_Invoices.Month) AND 
		(ProjectMonth.Project = qryJobMonth_Invoices.ProjectParent)) 
	LEFT JOIN qryJobMonthPortfolioSales ON 
		(ProjectMonth.MonthNo = qryJobMonthPortfolioSales.Month) AND 
		(ProjectMonth.Project = qryJobMonthPortfolioSales.PlanPortfolio)) 
	LEFT JOIN qryJobMonth_TotProfile ON 
		ProjectMonth.Project = qryJobMonth_TotProfile.Project

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_DeleteProjectMonth3    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_DeleteProjectMonth3    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[sp_DeleteProjectMonth3] AS
DELETE FROM ProjectMonth3

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_qryJobMonthCum] as
INSERT INTO 	ProjectMonth3
		( EndPeriod, 
		PeriodName, 
		Project, 
		CumCost, 
		CumInvoices, 
		CumCOIW, 
		CumPortSales, 
		CumProfile, 
		SumOfCostProfile, 
		SumOfMstoneDue, 
		SumOfDue__Done, 
		SumOfOnTime,
		CumCWDebit,
		CumCWCredit,
		CumTotalHours,
		CumSubcontracts,
		CumTestCosts,
		CumPayCosts
		 )
SELECT DISTINCT tblPeriod.EndPeriod,
		tblPeriod.PeriodName, 
		ProjectMonth2.Project, 
		SUM(ProjectMonth2.TotalCost) AS CumCost, 
		SUM(ProjectMonth2.Invoices) AS CumInvoices, 
		SUM(ProjectMonth2.COIW) AS CumCOIW, 
		SUM(ProjectMonth2.PortSales) AS CumPortSales, 
		SUM(ProjectMonth2.CostProfile) AS CumProfile, 
		ProjectMonth2.SumOfCostProfile, 
		SUM(ProjectMonth2.MstoneDue) AS SumOfMstoneDue, 
		SUM(ProjectMonth2.Due__Done)AS SumOfDue__Done, 
		SUM(ProjectMonth2.OnTime) AS SumOfOnTime,
		SUM(ProjectMonthCaseWork.CWDebit) AS CumCWDebit,
		SUM(ProjectMonthCasework.CWCredit) AS CumCWCredit,
		SUM(ProjectMonth2.TotalHours) AS CumTotalHours,
		SUM(ProjectMonth2.Subcontracts) AS CumSubcontracts,
		SUM(ProjectMonth2.TransferCosts) AS CumTestCosts,
		SUM(ProjectMonth2.PayCosts) AS CumPayCosts
FROM (tblPeriod 
	INNER JOIN tblkPeriodMonth ON tblPeriod.PeriodName = tblkPeriodMonth.PeriodName)
 	INNER JOIN ProjectMonth2 ON tblkPeriodMonth.MonthNo = ProjectMonth2.MonthNo
	INNER JOIN ProjectMonthCasework ON (ProjectMonth2.Monthno = ProjectMonthCasework.monthNo) 
	AND  (ProjectMonth2.Project = ProjectMonthCasework.Project) 
GROUP BY 	tblPeriod.EndPeriod, 
		tblPeriod.PeriodName, 
		ProjectMonth2.Project, 
		ProjectMonth2.SumOfCostProfile

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_qryJobMonth_Final] @Month int AS
INSERT INTO 	ProjectMonthFinal 
		( Project, 
		MonthNo, 
		CostProfile, 
		Subcontracts, 
		Animals, 
		NonAnimals, 
		TimeCosts, 
		TransferCosts, 
		TotalCost, 
		Invoices, 
		COIW, 
		PortSales, 
		CumCost, 
		CumProfile, 
		PeriodName, 
		SumOfCostProfile, 
		CumInvoices, 
		CumCOIW, 
		CumPortSales, 
		MstoneDue, 
		Due__Done, 
		OnTime, 
		SumOfMstoneDue, 
		SumOfDue__Done, 
		SumOfOnTime, 
		CumFlag,
		CWDebit,
		CWCredit,
		CumCWDebit,
		CumCWCredit,
		TotalHours,
		CumTotalHours,
		CumSubcontracts,
		CumTestCosts,
		PayCosts,
		CumPayCosts )

SELECT DISTINCT
	ProjectMonth2.Project, 
	ProjectMonth2.MonthNo, 
	ProjectMonth2.CostProfile, 
	ProjectMonth2.Subcontracts, 
	ProjectMonth2.Animals, 
	ProjectMonth2.NonAnimal, 
	ProjectMonth2.TimeCosts, 
	ProjectMonth2.TransferCosts, 
	ProjectMonth2.TotalCost, 
	ProjectMonth2.Invoices, 
	ProjectMonth2.COIW, 
	ProjectMonth2.PortSales, 
	CASE
		WHEN ProjectMonth2.monthno <= @month THEN CumCost * 1
	ELSE NULL
	END AS Expr1,
	ProjectMonth3.CumProfile, 	
	ProjectMonth3.PeriodName, 
	ProjectMonth3.SumOfCostProfile, 
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN CumInvoices * 1
		ELSE NULL
	END AS Expr2,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN CumCOIW * 1
		ELSE NULL
	END AS Expr3,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN CumPortSales * 1
		ELSE NULL
	END AS Expr4,
	ProjectMonth2.MstoneDue, 
	ProjectMonth2.Due__Done, 
	ProjectMonth2.OnTime, 
	ProjectMonth3.SumOfMstoneDue,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN SumOfDue__Done * 1
		ELSE NULL
	END AS Expr6,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN SumOfOnTime * 1
		ELSE NULL
	END AS Expr7,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1
		ELSE NULL
	END AS CumFlag,
	
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonthCasework.CWDebit
		ELSE NULL
	END,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonthCasework.CWCredit
		ELSE NULL
	END,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumCWDebit
		ELSE NULL
	END,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumCWCredit
		ELSE NULL
	END,
	ProjectMonth2.TotalHours,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumTotalHours
		ELSE NULL
	END,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumSubcontracts
		ELSE NULL
	END ,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumTestCosts
		ELSE NULL
	END,
	ProjectMonth2.PayCosts,
	CASE
		WHEN ProjectMonth2.monthno<= @month THEN 1 * ProjectMonth3.CumPayCosts
		ELSE NULL
	END
			
FROM 	ProjectMonth2 
	INNER JOIN ProjectMonth3 
	ON (ProjectMonth2.Project = ProjectMonth3.Project) 
	AND (ProjectMonth2.MonthNo = ProjectMonth3.EndPeriod)
	INNER JOIN ProjectMonthCasework 
	ON (ProjectMonth2.Project = ProjectMonthCasework.Project) 
	AND (ProjectMonth2.MonthNo = ProjectMonthCasework.MonthNo)

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_LogRecreateSummaries] @Month as smallint 
as
	set nocount on
	DECLARE	@Mno varchar(20)

	EXEC	[dbo].[sp_Get_SP_No]
		@Mno = @Mno OUTPUT

	INSERT Into RecreateSummaries_Log(UserID, Period, DateDone)
	Values(@Mno,@Month,getdate())

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_MO]
	@period int
as
delete from [dbo].[Period_MonthlyOutput]
where period=@period
INSERT INTO [dbo].[Period_MonthlyOutput]
(	 [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[SPC]
      ,[WorkGroup]
      ,[SCC]
      ,[TestCode]
      ,[Volume]
      ,[TestPrice]
      ,[TotalCost]
)
SELECT  @Period,
	tlkpProject.ParentProject AS Project, 
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end IsDefraProject, 
	CostCentre.ProfitCentre AS OPC, 
	CostCentre.CostCentre AS OCC, 
	MonthlyOutput.Month, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.WorkGroup, 
	WorkGroup.CostCentre AS SCC,  
	MonthlyOutput.TestCode, 
	MonthlyOutput.Volume, 
	tlkpTestReqmt.UnitPrice as TestPrice, 
	convert(money,[UnitPrice]*[Volume]) AS TotalCost

FROM ((tlkpProject LEFT JOIN CostCentre 
	ON tlkpProject.CostCentre = CostCentre.CostCentre) 
	INNER JOIN (MonthlyOutput 
	INNER JOIN WorkGroup ON MonthlyOutput.WorkGroup = WorkGroup.WorkGroup) 
	ON tlkpProject.ParentProject = MonthlyOutput.Buyer) 
	INNER JOIN tlkpTestReqmt 
	ON (MonthlyOutput.Buyer = tlkpTestReqmt.projectBuyerCode) 
	AND (MonthlyOutput.TestCode = tlkpTestReqmt.TestCode)

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_PSC]
	@period int
as
delete from [dbo].Period_Proj_SubContract
where period=@period
INSERT INTO [dbo].[Period_Proj_SubContract]
		([Period]
      ,[SubContCounter]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[Amount]
      ,[AcctCode])
Select @period,
     dbo.Proj_SubContract.SubContCounter, 
	dbo.Proj_SubContract.Project, 
	dbo.tlkpProject.OracleProjectCode, 
    dbo.tlkpProject.SubAccountCode, 
	CASE tlkpProject.IsDefraProject WHEN 0 THEN 'No' ELSE 'Yes' END AS IsDefraProject, 
	dbo.CostCentre.ProfitCentre AS OPC, 
    dbo.CostCentre.CostCentre AS OCC, 
	dbo.Proj_SubContract.Month, 
	dbo.Proj_SubContract.Amount, 
	dbo.Proj_SubContract.AcctCode

FROM         dbo.CostCentre RIGHT OUTER JOIN
                      dbo.tlkpProject ON dbo.CostCentre.CostCentre = dbo.tlkpProject.CostCentre INNER JOIN
                      dbo.Proj_SubContract ON dbo.tlkpProject.ParentProject = dbo.Proj_SubContract.Project

GO

USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_TCC]
	@period int
as
delete from [dbo].[Period_TimeCostCalcs]
where period=@period

INSERT INTO .[dbo].[Period_TimeCostCalcs]
(
      [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[Month]
      ,[DefraProject]
      ,[OCC]
      ,[OPC]
      ,[SPC]
      ,[SCC]
      ,[Name]
      ,[GradeCode]
      ,[SPNumber]
      ,[ChargeRate]
      ,[Pay]
      ,[Nonpay]
      ,[Overhead]
      ,[Time]
      ,[TotalCost])

SELECT @Period, 
	tlkpProject.ParentProject AS Project,	
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	TimeCostCalcs.Month, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end AS DefraProject, 
	CostCentre.CostCentre AS OCC, 
	CostCentre.ProfitCentre AS OPC, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.CostCentre AS SCC, 
	TimeCostCalcs.Name, 
	TimeCostCalcs.GradeCode, 
	tblWGEmployee.SPNumber, 
	TimeCostCalcs.ChargeRate, 
	TimeCostCalcs.Pay, 
	TimeCostCalcs.Nonpay, 
	TimeCostCalcs.Overhead, 
	TimeCostCalcs.Time, 
	TimeCostCalcs.Cost AS TotalCost
FROM dbo.tblWGEmployee INNER JOIN ((tlkpProject LEFT JOIN CostCentre ON tlkpProject.CostCentre = CostCentre.CostCentre) INNER JOIN (TimeCostCalcs INNER JOIN WorkGroup ON TimeCostCalcs.WorkGroup = WorkGroup.WorkGroup) ON tlkpProject.ParentProject = TimeCostCalcs.Project) ON tblWGEmployee.PACTid = TimeCostCalcs.StaffID

GO
