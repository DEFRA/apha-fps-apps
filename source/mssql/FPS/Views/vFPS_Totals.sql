USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vFPS_Totals]
AS
SELECT     dbo.FPSYearTotals.ParentProject, dbo.FPSYearTotals.Program, dbo.FPSYearTotals.TotalAdditionalCosts, dbo.FPSYearTotals.TotalAnimalCosts, 
                      dbo.FPSYearTotals.TotalStaffCosts, dbo.FPSYearTotals.TotalTestCosts, dbo.FPSYearTotals.TotalCosts, dbo.FPSYearTotals.CustIncome, 
                      dbo.FPSYearTotals.TransferIncome, dbo.FPSYearTotals.TotalIncome, dbo.FPSYearTotals.Budget_CVL, dbo.FPSYearTotals.RequiredProfit, 
                      dbo.FPSYearTotals.Manager, dbo.FPSYearTotals.Customer, dbo.FPSYearTotals.ProjectStatus, dbo.FPSYearTotals.PVSIncome, 
                      dbo.FPSYearTotals.PlanCaseworkDebit, MA_A.BFBudget AS MA_BFBudget
FROM         dbo.FPSYearTotals LEFT OUTER JOIN 
	(Select Project,BFBudget from  MAB_Archive.dbo.MY_tlkpProjectRadTrackData
                      
WHERE     (MAB_Archive.dbo.MY_tlkpProjectRadTrackData.Year =
                          (SELECT     RIGHT(DB_Var_Value, 4) AS Expr1
                            FROM          dbo.tblDB_Variables
                            WHERE      (DB_Var_Name = 'DB_Name'))) ) as MA_A 
on dbo.FPSYearTotals.ParentProject = MA_A.Project

GO
