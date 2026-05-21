USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vFiveYearProjectSummary_Sub]
AS
SELECT     dbo.MY_ProjectMonthFinal.Year, dbo.MY_ProjectMonthFinal.Project, dbo.MY_tlkpProject.CustIncome AS CumBudget, 
                      SUM(dbo.MY_ProjectMonthFinal.TotalCost) AS CumCost
FROM         dbo.MY_ProjectMonthFinal INNER JOIN
                      dbo.MY_tlkpProject ON dbo.MY_ProjectMonthFinal.Year = dbo.MY_tlkpProject.Year AND 
                      dbo.MY_ProjectMonthFinal.Project = dbo.MY_tlkpProject.ParentProject
GROUP BY dbo.MY_ProjectMonthFinal.Year, dbo.MY_ProjectMonthFinal.Project, dbo.MY_tlkpProject.CustIncome
HAVING      (dbo.MY_ProjectMonthFinal.Year >= 2004)

GO
