USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryJobMonth_SubContracts] as 
SELECT Project, Month, Sum(Animals1) AS Animals, Sum(Other1) AS Other, 
Sum(Animals1) + Sum(Other1) AS Total
FROM qryJobMonth_SubContracts1
GROUP BY Project, Month

GO
