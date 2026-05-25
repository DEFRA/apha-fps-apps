USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPactProfitcentreGrade    Script Date: 3/4/00 1:48:17 PM ******/
/****** Object:  View dbo.vPactProfitcentreGrade    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vPactProfitcentreGrade] AS
SELECT PCGrade as PC_Grade,
DivisionGrade,
GradeCode,
ProfitCentre,
ChargeRate,
DirectRate,
PayRate,
NPR,
OHR,
HrsAvailable
FROM ProfitCentreGrade

GO
