USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.qryTestsPCCostplan_Crosstab
AS
SELECT TOP (100) PERCENT dbo.qryTestPCCCostPlan.JobCode, dbo.qryTestPCCCostPlan.TestCode, dbo.qryTestPCCCostPlan.ProfitCentre, 
               ((CASE qryTestPCCCostplan.ProfitCentre WHEN 'LabT' THEN MAX(COALESCE (tblTestRequirementRCCost.Price, qryTestPCCCostplan.Price)) ELSE 0 END)) 
               AS LabT, ((CASE qryTestPCCCostplan.ProfitCentre WHEN 'VSD GB' THEN MAX(COALESCE (tblTestRequirementRCCost.Price, qryTestPCCCostplan.Price)) 
               ELSE 0 END)) AS VetR, ((CASE qryTestPCCCostplan.ProfitCentre WHEN 'Viro' THEN MAX(COALESCE (tblTestRequirementRCCost.Price, 
               qryTestPCCCostplan.Price)) ELSE 0 END)) AS Viro
FROM  dbo.qryTestPCCCostPlan LEFT OUTER JOIN
               dbo.tblTestRequirementRCCost ON dbo.qryTestPCCCostPlan.ProfitCentre = dbo.tblTestRequirementRCCost.ProfitCentre AND 
               dbo.qryTestPCCCostPlan.JobCode = dbo.tblTestRequirementRCCost.Buyer AND 
               dbo.qryTestPCCCostPlan.TestCode = dbo.tblTestRequirementRCCost.TestCode
GROUP BY dbo.qryTestPCCCostPlan.JobCode, dbo.qryTestPCCCostPlan.TestCode, dbo.qryTestPCCCostPlan.ProfitCentre
ORDER BY dbo.qryTestPCCCostPlan.JobCode

GO
