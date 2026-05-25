USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPlanCatWGGrade_General    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.vPlanCatWGGrade_General    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vPlanCatWGGrade_General] AS
SELECT	PlanCategory,
	WGGrade	
FROM	PlanCatWGGrade
WITH CHECK OPTION

GO
