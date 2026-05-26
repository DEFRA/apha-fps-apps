USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtblkpProfitCentre_General    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtblkpProfitCentre_General    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblkpProfitCentre_General] AS
SELECT	ProfitCentre,
	ProfitCentreName
	
FROM	tblkpProfitcentre

GO
