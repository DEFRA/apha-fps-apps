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
