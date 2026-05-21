USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_TransferUnion    Script Date: 3/4/00 1:48:19 PM ******/
CREATE VIEW [dbo].[qryJobMonth_TransferUnion] AS
(SELECT Project,
 Month,
 TransferCost
FROM qryJobMonth_TCTransfers)
 UNION ALL
(SELECT Project,
 Month,
 TransferCost
FROM qryJobMonth_Transfers1)

GO
