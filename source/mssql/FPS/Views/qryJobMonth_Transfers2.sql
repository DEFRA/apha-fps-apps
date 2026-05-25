USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Transfers2    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.qryJobMonth_Transfers2    Script Date: 1/12/99 12:13:47 PM ******/
/****** Object:  View dbo.qryJobMonth_Transfers2    Script Date: 10/27/98 11:55:27 AM ******/
CREATE VIEW [dbo].[qryJobMonth_Transfers2] as
SELECT DISTINCT Project, Month, Sum(TransferCost) AS SumOfTransferCost
FROM qryJobMonth_Transfers1
GROUP BY Project, Month

GO
