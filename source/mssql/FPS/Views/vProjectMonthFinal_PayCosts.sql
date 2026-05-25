USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectMonthFinal_PayCosts]
AS
SELECT     dbo.ProjectMonthFinal.MonthNo, dbo.ProjectMonthFinal.PeriodName, dbo.ProjectMonthFinal.CumFlag, dbo.ProjectMonthFinal.CostProfile, 
                      dbo.ProjectMonthFinal.Subcontracts, dbo.ProjectMonthFinal.Animals, dbo.ProjectMonthFinal.NonAnimals, dbo.ProjectMonthFinal.TimeCosts, 
                      dbo.ProjectMonthFinal.TransferCosts, 
                      dbo.ProjectMonthFinal.TotalCost - dbo.ProjectMonthFinal.TimeCosts + dbo.ProjectMonthFinal.PayCosts AS TotalCost, dbo.ProjectMonthFinal.Invoices, 
                      dbo.ProjectMonthFinal.COIW, dbo.ProjectMonthFinal.PortSales, ISNULL(dbo.ProjectMonthFinal.CumCost, 0) 
                      + ISNULL(dbo.ProjectMonthFinal.CumPayCosts, 0) - ISNULL(SUM(dbo.TimeCostCalcs.Cost), 0) AS CumCost, dbo.ProjectMonthFinal.CumProfile, 
                      dbo.ProjectMonthFinal.SumOfCostProfile, dbo.ProjectMonthFinal.CumInvoices, dbo.ProjectMonthFinal.CumCOIW, dbo.ProjectMonthFinal.CumPortSales, 
                      dbo.ProjectMonthFinal.MstoneDue, dbo.ProjectMonthFinal.Due__Done, dbo.ProjectMonthFinal.OnTime, dbo.ProjectMonthFinal.SumOfMstoneDue, 
                      dbo.ProjectMonthFinal.SumOfDue__Done, dbo.ProjectMonthFinal.SumOfOnTime, dbo.ProjectMonthFinal.CWDebit, dbo.ProjectMonthFinal.CWCredit, 
                      dbo.ProjectMonthFinal.CumCWDebit, dbo.ProjectMonthFinal.CumCWCredit, dbo.ProjectMonthFinal.TotalHours, dbo.ProjectMonthFinal.CumTotalHours, 
                      dbo.ProjectMonthFinal.CumSubContracts, dbo.ProjectMonthFinal.CumTestCosts, dbo.ProjectMonthFinal.Project, 
                      dbo.ProjectMonthFinal.PayCosts AS PayRateCosts
FROM         dbo.ProjectMonthFinal LEFT OUTER JOIN
                      dbo.TimeCostCalcs ON dbo.ProjectMonthFinal.Project = dbo.TimeCostCalcs.Project AND 
                      dbo.ProjectMonthFinal.MonthNo = dbo.TimeCostCalcs.Month
GROUP BY dbo.ProjectMonthFinal.MonthNo, dbo.ProjectMonthFinal.PeriodName, dbo.ProjectMonthFinal.CumFlag, dbo.ProjectMonthFinal.CostProfile, 
                      dbo.ProjectMonthFinal.Subcontracts, dbo.ProjectMonthFinal.Animals, dbo.ProjectMonthFinal.NonAnimals, dbo.ProjectMonthFinal.TimeCosts, 
                      dbo.ProjectMonthFinal.TransferCosts, dbo.ProjectMonthFinal.TotalCost, dbo.ProjectMonthFinal.Invoices, dbo.ProjectMonthFinal.COIW, 
                      dbo.ProjectMonthFinal.PortSales, dbo.ProjectMonthFinal.CumProfile, dbo.ProjectMonthFinal.SumOfCostProfile, dbo.ProjectMonthFinal.CumInvoices, 
                      dbo.ProjectMonthFinal.CumCOIW, dbo.ProjectMonthFinal.CumPortSales, dbo.ProjectMonthFinal.MstoneDue, dbo.ProjectMonthFinal.Due__Done, 
                      dbo.ProjectMonthFinal.OnTime, dbo.ProjectMonthFinal.SumOfMstoneDue, dbo.ProjectMonthFinal.SumOfDue__Done, 
                      dbo.ProjectMonthFinal.SumOfOnTime, dbo.ProjectMonthFinal.CWDebit, dbo.ProjectMonthFinal.CWCredit, dbo.ProjectMonthFinal.CumCWDebit, 
                      dbo.ProjectMonthFinal.CumCWCredit, dbo.ProjectMonthFinal.TotalHours, dbo.ProjectMonthFinal.CumTotalHours, 
                      dbo.ProjectMonthFinal.CumSubContracts, dbo.ProjectMonthFinal.CumTestCosts, dbo.ProjectMonthFinal.Project, 
                      dbo.ProjectMonthFinal.TotalCost - dbo.ProjectMonthFinal.TimeCosts + dbo.ProjectMonthFinal.PayCosts, dbo.ProjectMonthFinal.CumCost, 
                      dbo.ProjectMonthFinal.CumPayCosts, dbo.ProjectMonthFinal.PayCosts

GO
