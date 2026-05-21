USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_TCtransfers    Script Date: 3/4/00 1:48:18 PM ******/
CREATE VIEW [dbo].[qryJobMonth_TCtransfers] AS
SELECT vPacttlkpTestCapability.PlanPortfolio AS Project,
 MonthlyOutput.Month, 
MonthlyOutput.TestCode, 
MonthlyOutput.Volume, 
tlkpTestReqmt.UnitPrice AS IntUnitPrice, 
Sum(Volume*UnitPrice) AS TransferCost
FROM (MonthlyOutput 
INNER JOIN tlkpTestReqmt ON (MonthlyOutput.TestCode = tlkpTestReqmt.TestCode) AND (MonthlyOutput.Buyer = tlkpTestReqmt.Buyer)) 
INNER JOIN vPacttlkpTestCapability ON tlkpTestReqmt.Buyer = vpacttlkpTestCapability.WGTestCode
GROUP BY vpacttlkpTestCapability.PlanPortfolio, MonthlyOutput.Month, MonthlyOutput.TestCode, MonthlyOutput.Volume, tlkpTestReqmt.UnitPrice

GO
