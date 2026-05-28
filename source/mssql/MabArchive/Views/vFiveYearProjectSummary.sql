USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vFiveYearProjectSummary]
AS
SELECT     dbo.vFiveYearProjectSummary_Sub2.Project, dbo.vFiveYearProjectSummary_Sub2.Year, dbo.vFiveYearProjectSummary_Sub2.DisplayYear, 
                      dbo.vFiveYearProjectSummary_Sub2.CustIncome, dbo.vFiveYearProjectSummary_Sub2.VLAExpeniture, 
                      dbo.vFiveYearProjectSummary_Sub2.IncomeLessCost, dbo.vFiveYearProjectSummary_Sub2.InvoicedIncome, 
                      dbo.vFiveYearProjectSummary_Sub2.InvoicesLessCost, dbo.vFiveYearProjectSummary_Sub2.Budget, 
                      dbo.vFiveYearProjectSummary_Sub2.BudgetRemaining, SUM(dbo.vFiveYearProjectSummary_Sub.CumBudget) AS CumBudget, 
                      SUM(dbo.vFiveYearProjectSummary_Sub.CumCost) AS CumCost
FROM         dbo.vFiveYearProjectSummary_Sub2 INNER JOIN
                      dbo.vFiveYearProjectSummary_Sub ON dbo.vFiveYearProjectSummary_Sub2.Project = dbo.vFiveYearProjectSummary_Sub.Project AND 
                      dbo.vFiveYearProjectSummary_Sub2.Year >= dbo.vFiveYearProjectSummary_Sub.Year
GROUP BY dbo.vFiveYearProjectSummary_Sub2.Project, dbo.vFiveYearProjectSummary_Sub2.Year, dbo.vFiveYearProjectSummary_Sub2.DisplayYear, 
                      dbo.vFiveYearProjectSummary_Sub2.CustIncome, dbo.vFiveYearProjectSummary_Sub2.VLAExpeniture, 
                      dbo.vFiveYearProjectSummary_Sub2.IncomeLessCost, dbo.vFiveYearProjectSummary_Sub2.InvoicedIncome, 
                      dbo.vFiveYearProjectSummary_Sub2.InvoicesLessCost, dbo.vFiveYearProjectSummary_Sub2.Budget, 
                      dbo.vFiveYearProjectSummary_Sub2.BudgetRemaining

GO
