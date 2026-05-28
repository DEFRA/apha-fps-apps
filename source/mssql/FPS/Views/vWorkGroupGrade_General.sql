USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vWorkGroupGrade_General    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vWorkGroupGrade_General    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vWorkGroupGrade_General] AS
SELECT 	WGGrade,
	ProfitCentreGrade,
	GradeCode,
	WorkGroup
	
 
FROM	WorkGroupGrade

GO
