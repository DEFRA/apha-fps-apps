USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vWorkGroup    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vWorkGroup    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vWorkGroup] AS
SELECT	*
FROM	WorkGroup
WHERE	WorkGroup.ProfitCentre IN (SELECT vtblkpProfitCentre.ProfitCentre FROM vtblkpProfitCentre)
WITH CHECK OPTION

GO
