USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vProfitCentreGrade_General    Script Date: 3/4/00 1:48:17 PM *****
***** Object:  View dbo.vProfitCentreGrade_General    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vProfitCentreGrade_General]
AS
SELECT     PCGrade, DivisionGrade, GradeCode, ProfitCentre, ChargeRate, DefraChargeRate
FROM         dbo.ProfitCentreGrade

GO
