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
