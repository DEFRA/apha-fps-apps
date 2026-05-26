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
