USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryTotalAdditionalCosts    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[qryTotalAdditionalCosts] AS
SELECT DISTINCT tblAdditionalCosts.JobCode, 
Sum(tblAdditionalCosts.ItemCost) AS TotalAdditionalCosts
FROM tblAdditionalCosts
GROUP BY tblAdditionalCosts.JobCode

GO
