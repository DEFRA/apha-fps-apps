USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Invoices    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.qryJobMonth_Invoices    Script Date: 1/12/99 12:13:47 PM ******/
/****** Object:  View dbo.qryJobMonth_Invoices    Script Date: 10/27/98 11:55:26 AM ******/
CREATE VIEW [dbo].[qryJobMonth_Invoices] as
SELECT Proj_Invoice.ProjectParent, Proj_Invoice.Month, 
Sum(Proj_Invoice.Amount) AS SumOfAmount1, Sum(Proj_Invoice.CostOfWork) AS WorkCost
FROM Proj_Invoice
GROUP BY Proj_Invoice.ProjectParent, Proj_Invoice.Month

GO
