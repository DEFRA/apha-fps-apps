USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vProfitCentreGrade    Script Date: 3/4/00 1:48:17 PM *****
***** Object:  View dbo.vProfitCentreGrade    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vProfitCentreGrade]
WITH  VIEW_METADATA
AS
SELECT     PCGrade, DivisionGrade, GradeCode, ProfitCentre, ChargeRate, DirectRate, PayRate, NPR, OHR, HrsAvailable, OldChargeRate, DefraChargeRate
FROM         dbo.ProfitCentreGrade
WHERE     (ProfitCentre IN
                          (SELECT     ProfitCentre
                            FROM          dbo.vtblkpProfitCentre))
WITH CHECK OPTION

GO
