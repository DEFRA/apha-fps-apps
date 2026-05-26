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
