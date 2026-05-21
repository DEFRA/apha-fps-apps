USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPlanCatWGGrade    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.vPlanCatWGGrade    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vPlanCatWGGrade] AS
SELECT	PlanCategory,
	WGGrade,
	Hours,
	CreatedBy,
	SellerAgrees,
	BuyerAgrees
FROM	PlanCatWGGrade
WHERE	PlanCatWGGrade.WGGrade IN (SELECT vWorkGroupGrade.WGGrade FROM vWorkGroupGrade)
	
WITH CHECK OPTION

GO
