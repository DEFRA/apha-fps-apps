USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vWorkGroupGrade    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vWorkGroupGrade    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vWorkGroupGrade] AS
SELECT	WGGrade,
	ProfitCentreGrade,
	GradeCode,
	WorkGroup,
	ChargeRateWG,
	DirectRateWG,
	PayRateWG,
	NPRWG,
	OHRWG,
	AvSalary,
	HrsChangedBy
FROM	WorkGroupGrade
WHERE	WorkGroupGrade.WorkGroup IN (SELECT vWorkGroup.WorkGroup FROM vWorkGroup)
WITH CHECK OPTION

GO
