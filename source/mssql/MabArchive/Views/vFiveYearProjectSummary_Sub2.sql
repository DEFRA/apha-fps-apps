USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vFiveYearProjectSummary_Sub2]
AS
SELECT     dbo.MY_tlkpProject.ParentProject AS Project, dbo.MY_tlkpProject.Year, CAST(dbo.MY_tlkpProject.Year AS char(4)) 
                      + '/' + RIGHT(CAST(dbo.MY_tlkpProject.Year + 1 AS char(4)), 2) AS DisplayYear, dbo.MY_tlkpProject.CustIncome, 
                      dbo.MY_ProjectMonthFinal.CumCost AS VLAExpeniture, dbo.MY_tlkpProject.CustIncome - dbo.MY_ProjectMonthFinal.CumCost AS IncomeLessCost, 
                      dbo.MY_ProjectMonthFinal.CumInvoices AS InvoicedIncome, 
                      dbo.MY_ProjectMonthFinal.CumInvoices - dbo.MY_ProjectMonthFinal.CumCost AS InvoicesLessCost, dbo.MY_tlkpProject.Budget_CVL AS Budget, 
                      dbo.MY_tlkpProject.Budget_CVL - dbo.MY_ProjectMonthFinal.CumCost AS BudgetRemaining
FROM         dbo.MY_ProjectMonthFinal INNER JOIN
                      dbo.MY_tlkpProject ON dbo.MY_ProjectMonthFinal.Year = dbo.MY_tlkpProject.Year AND 
                      dbo.MY_ProjectMonthFinal.Project = dbo.MY_tlkpProject.ParentProject INNER JOIN
                          (SELECT     Year, MAX(MonthNo) AS LatestMonth
                            FROM          dbo.MY_ProjectMonthFinal AS MY_ProjectMonthFinal_1
                            WHERE      (CumFlag = 1)
                            GROUP BY Year) AS L ON dbo.MY_ProjectMonthFinal.Year = L.Year AND dbo.MY_ProjectMonthFinal.MonthNo = L.LatestMonth CROSS JOIN
                      dbo.vLatestMonthYear
WHERE     (dbo.MY_tlkpProject.Year BETWEEN dbo.vLatestMonthYear.Year - 5 AND (CASE WHEN RIGHT(dbo.MY_tlkpProject.PROGRAM, 4) 
                      = '_Res' THEN dbo.vLatestMonthYear.Year - 1 WHEN RIGHT(dbo.MY_tlkpProject.PROGRAM, 5) 
                      = '_SURV' THEN dbo.vLatestMonthYear.Year - 1 WHEN dbo.MY_tlkpProject.PROGRAM = 'OM_WORK' THEN dbo.vLatestMonthYear.Year - 1 ELSE dbo.vLatestMonthYear.Year
                       END))

GO
