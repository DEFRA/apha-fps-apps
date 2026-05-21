USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_TransfersTotal    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[qryJobMonth_TransfersTotal] AS
SELECT DISTINCT Project, Month, Sum(TransferCost) AS SumOfTransferCost
FROM qryJobMonth_TransferUnion
GROUP BY Project, Month

GO
