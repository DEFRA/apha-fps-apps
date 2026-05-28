USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Searching for double quotes ******/
/****** As above plus... ******/

CREATE VIEW [dbo].[qryCaseWorkHours] AS
SELECT TimeCostCalcs.Project, TimeCostCalcs.Month, Sum(TimeCostCalcs.Cost) AS SumOfCost
FROM TimeCostCalcs INNER JOIN tlkpJobCode ON (TimeCostCalcs.Project = tlkpJobCode.ParentProject) AND (TimeCostCalcs.JobCode = tlkpJobCode.JobCode)
WHERE ((tlkpJobCode.Type='casework'))
GROUP BY TimeCostCalcs.Project, TimeCostCalcs.Month

GO
