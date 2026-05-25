USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Transfers1    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.qryJobMonth_Transfers1    Script Date: 1/12/99 12:13:46 PM ******/
/****** Object:  View dbo.qryJobMonth_Transfers1    Script Date: 10/27/98 11:55:27 AM ******/
CREATE VIEW [dbo].[qryJobMonth_Transfers1] as 
SELECT DISTINCT MonthlyOutput.Buyer AS Project, MonthlyOutput.Month, MonthlyOutput.TestCode, 
MonthlyOutput.Volume, tlkpTestReqmt.UnitPrice AS IntUnitPrice, 
Sum(Volume * tlkpTestReqmt.UnitPrice) AS TransferCost
FROM (TestOrProduct INNER JOIN tlkpTestReqmt ON TestOrProduct.ItemCode = tlkpTestReqmt.TestCode)
INNER JOIN MonthlyOutput ON (tlkpTestReqmt.Buyer = MonthlyOutput.Buyer) AND 
(tlkpTestReqmt.TestCode = MonthlyOutput.TestCode)
GROUP BY MonthlyOutput.Buyer, MonthlyOutput.Month, MonthlyOutput.TestCode, 
MonthlyOutput.Volume, tlkpTestReqmt.UnitPrice

GO
