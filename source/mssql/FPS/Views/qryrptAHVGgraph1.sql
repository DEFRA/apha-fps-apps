USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryrptAHVGgraph1    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.qryrptAHVGgraph1    Script Date: 1/12/99 12:13:48 PM ******/
/****** Object:  View dbo.qryrptAHVGgraph1    Script Date: 10/27/98 11:55:27 AM ******/
/****** Object:  View dbo.qryrptAHVGgraph1    Script Date: 16/10/97 12:35:41 ******/
CREATE VIEW [dbo].[qryrptAHVGgraph1] as
SELECT 	 
	MonthlyOutput.Buyer, 
	tblkPeriodMonth.EndMonth, 
	MonthlyOutput.TestCode, 
	Sum(MonthlyOutput.Volume) AS ActualVol
FROM tlkpTestCapability 
INNER JOIN (tblkPeriodMonth 
INNER JOIN MonthlyOutput ON tblkPeriodMonth.MonthNo = MonthlyOutput.Month)
	ON (tlkpTestCapability.WorkGroup = MonthlyOutput.WorkGroup) 
	AND (tlkpTestCapability.TestCode = MonthlyOutput.TestCode)
GROUP BY 	
		MonthlyOutput.Buyer, 
		tblkPeriodMonth.EndMonth, 
		MonthlyOutput.TestCode

GO
