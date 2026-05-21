USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vTestProjectMonthFinal    Script Date: 3/4/00 1:48:15 PM ******/
/****** Object:  View dbo.vTestProjectMonthFinal    Script Date: 1/12/99 12:13:46 PM ******/
CREATE VIEW [dbo].[vTestProjectMonthFinal] AS
SELECT		MonthNo,
		/*PeriodName,
		CumFlag,*/
		Sum(CostProfile) AS CostProfile,
		Sum(Subcontracts) AS Subcontracts,
		Sum(Animals) AS Animals,
		Sum(NonAnimals) AS NonAnimals,
		Sum(TimeCosts) AS TimeCosts,
		Sum(TransferCosts) AS TransferCosts,
		Sum(TotalCost) AS TotalCost,
		Sum(Invoices) AS Invoices,
		Sum(COIW) AS COIW,
		Sum(PortSales) AS PortSales,
		Sum(CumCost) AS CumCost,
		Sum(CumProfile) AS CumProfile,
		Sum(SumOfCostProfile) AS SumOfCostProfile,
		Sum(CumInvoices) AS CumInvoices,
		Sum(CumCOIW) AS CumCOIW,
		Sum(CumPortSales) AS CumPortSales,
		Sum(MstoneDue) AS MstoneDue,
		Sum(Due__Done) AS Due__Done,
		Sum(OnTime) AS OnTime,
		Sum(SumOfMstoneDue) AS SumOfMstoneDue,
		Sum(SumOfDue__Done) AS SumOfDue__Done,
		Sum(SumOfOnTime) AS SumOfOnTime
FROM		ProjectMonthFinal
GROUP BY	MonthNo
WITH CUBE

GO
