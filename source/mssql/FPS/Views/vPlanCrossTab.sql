USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vPlanCrossTab]
AS
SELECT     JobCode, TestCode, SUM(LabT) AS LabT, SUM(VetR) AS VetR, SUM(Viro) AS Viro
FROM         dbo.qryTestsPCCostplan_Crosstab
GROUP BY JobCode, TestCode

GO
