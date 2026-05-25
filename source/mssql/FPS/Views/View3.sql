USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View3]
AS
SELECT     dbo.TimeCostCalcs.Project, dbo.TimeCostCalcs.JobCode, SUM(dbo.TimeCostCalcs.Cost) AS SumOfCost, 
                      CASE dbo.WorkGroup.ProfitCentre WHEN 'Path' THEN 'Surveillance/Pathology' WHEN 'vetr' THEN 'Surveillance/Pathology' ELSE 'Laboratory Testing' END
                       AS ResourceCentre
FROM         dbo.TimeCostCalcs INNER JOIN
                      dbo.WorkGroup ON dbo.TimeCostCalcs.WorkGroup = dbo.WorkGroup.WorkGroup
GROUP BY dbo.WorkGroup.ProfitCentre, dbo.TimeCostCalcs.Project, dbo.TimeCostCalcs.JobCode, 
                      CASE dbo.WorkGroup.ProfitCentre WHEN 'Path' THEN 'Surveillance/Pathology' WHEN 'vetr' THEN 'Surveillance/Pathology' ELSE 'Laboratory Testing' END
HAVING      (dbo.TimeCostCalcs.Project = 'TG0100') AND (MAX(dbo.TimeCostCalcs.Month) <=
                          (SELECT     MAX(EndPeriod) AS EndPeriod
                            FROM          tblPeriod
                            WHERE      FinalSummariesRun = - 1))

GO
