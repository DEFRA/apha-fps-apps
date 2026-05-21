USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vPactProjectYearCosts]
AS
SELECT     dbo.MY_ProjectMonthFinal.Project, CASE useprojectyear WHEN - 1 THEN DatePart(yyyy, DATEADD(m, 
                      dbo.MY_ProjectMonthFinal.MonthNo + 3 - DATEPART(M, dbo.G_tlkpProject_RadTrackData.StartDate), CONVERT(DATETIME, 
                      CAST(dbo.MY_ProjectMonthFinal.Year AS char(4)) + '-01-01 00:00:00', 102))) ELSE dbo.MY_ProjectMonthFinal.Year END AS Year, 
                      dbo.MY_ProjectMonthFinal.MonthNo, SUM(dbo.MY_ProjectMonthFinal.Subcontracts) AS SubContracts, SUM(dbo.MY_ProjectMonthFinal.Animals) 
                      AS Animals, SUM(dbo.MY_ProjectMonthFinal.TransferCosts) AS Tests, SUM(dbo.vTCC_Summary.Pay) AS Pay, 
                      SUM(dbo.vTCC_Summary.NonPay + dbo.vTCC_Summary.Overhead) AS NonPayOH, SUM(dbo.MY_ProjectMonthFinal.TotalHours) AS Hours, 
                      SUM(dbo.MY_ProjectMonthFinal.TotalCost) AS TotalCosts, SUM(dbo.MY_ProjectMonthFinal.TimeCosts) AS TimeCost
FROM         dbo.MY_ProjectMonthFinal LEFT OUTER JOIN
                      dbo.G_tlkpProject_RadTrackData ON dbo.MY_ProjectMonthFinal.Project = dbo.G_tlkpProject_RadTrackData.ParentProject LEFT OUTER JOIN
                      dbo.vTCC_Summary ON dbo.MY_ProjectMonthFinal.Year = dbo.vTCC_Summary.Year AND 
                      dbo.MY_ProjectMonthFinal.Project = dbo.vTCC_Summary.Project AND dbo.MY_ProjectMonthFinal.MonthNo = dbo.vTCC_Summary.Month
GROUP BY dbo.MY_ProjectMonthFinal.Project, dbo.MY_ProjectMonthFinal.MonthNo, dbo.G_tlkpProject_RadTrackData.UseProjectYear, 
                      dbo.MY_ProjectMonthFinal.Year, dbo.G_tlkpProject_RadTrackData.StartDate

GO
