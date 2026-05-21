USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.qryTestsPCCostplan_xtab
AS
SELECT TestCode, SUM(CASE ProfitCentre WHEN 'LabT' THEN Price ELSE 0 END) AS LabT, SUM(CASE ProfitCentre WHEN 'VSD GB' THEN Price ELSE 0 END) AS VetR, 
               SUM(CASE ProfitCentre WHEN 'Viro' THEN Price ELSE 0 END) AS Viro
FROM  dbo.tblTestRCCost
GROUP BY TestCode

GO
