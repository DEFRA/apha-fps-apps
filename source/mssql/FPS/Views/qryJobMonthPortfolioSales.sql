USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonthPortfolioSales    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.qryJobMonthPortfolioSales    Script Date: 1/12/99 12:13:48 PM ******/
/****** Object:  View dbo.qryJobMonthPortfolioSales    Script Date: 10/27/98 11:55:26 AM ******/
CREATE VIEW [dbo].[qryJobMonthPortfolioSales] as
SELECT DISTINCT tlkpTestCapability.PlanPortfolio, MonthlyOutput.Month, 
Sum(unitprice * volume) AS Fee
FROM tlkpTestReqmt INNER JOIN (tlkpTestCapability INNER JOIN MonthlyOutput ON 
(tlkpTestCapability.WorkGroup = MonthlyOutput.WorkGroup) AND 
(tlkpTestCapability.TestCode = MonthlyOutput.TestCode)) ON 
(tlkpTestReqmt.Buyer = MonthlyOutput.Buyer) AND 
(tlkpTestReqmt.TestCode = MonthlyOutput.TestCode)
GROUP BY tlkpTestCapability.PlanPortfolio, MonthlyOutput.Month

GO
