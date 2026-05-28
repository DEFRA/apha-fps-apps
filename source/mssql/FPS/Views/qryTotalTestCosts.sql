USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryTotalTestCosts    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[qryTotalTestCosts] AS
SELECT DISTINCT vtblTestRequ.JobCode, Sum(NoTests*TestPrice) AS TotalTestCosts
FROM vtblTestRequ
GROUP BY vtblTestRequ.JobCode

GO
