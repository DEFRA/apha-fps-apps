USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtblAdditionalCosts    Script Date: 3/4/00 1:48:17 PM ******/
/****** Object:  View dbo.vtblAdditionalCosts    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblAdditionalCosts_ByGroup] AS
SELECT 	JobCode,
	Account,
	Description,
	ItemCost,
	Freq,
	Supplier
FROM	tblAdditionalCosts
WHERE	tblAdditionalCosts.JobCode IN (SELECT ParentProject FROM vtlkpProject_ByGroup)
WITH CHECK OPTION

GO
