USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPactWorkGroupGrade    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vPactWorkGroupGrade    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vPactWorkGroupGrade] AS
SELECT  WorkGroupGrade.WGGrade AS WG_Grade, WorkGroupGrade.ProfitCentreGrade, WorkGroupGrade.GradeCode, WorkGroupGrade.WorkGroup, WorkGroupGrade.ChargeRateWG AS ChargeRate_WG, WorkGroupGrade.DirectRateWG AS DirectRate_WG, WorkGroupGrade.PayRateWG AS PayRate_WG, WorkGroupGrade.NPRWG AS NPR_WG, WorkGroupGrade.OHRWG AS OHR_WG, WorkGroupGrade.AvSalary, WorkGroupGrade.HrsChangedBy
FROM WorkGroupGrade

GO
