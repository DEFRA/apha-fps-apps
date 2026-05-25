USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view vTimeRecordedRC as
SELECT dbo.TimeCostCalcs.Project, dbo.WorkGroup.ProfitCentre
FROM dbo.WorkGroup INNER JOIN dbo.TimeCostCalcs ON dbo.WorkGroup.WorkGroup = dbo.TimeCostCalcs.WorkGroup
GROUP BY dbo.TimeCostCalcs.Project, dbo.WorkGroup.ProfitCentre;

GO
