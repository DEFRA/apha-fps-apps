USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryMilestone1]
AS
SELECT DISTINCT 
    Project, MilestoneRef, PlanDate, ActualDate, 
    MonthNoFin AS DueMonth, 
    CASE WHEN actualdate <= PlanDate THEN 1 ELSE 0 END AS OnTimeFlag,
     CASE WHEN ActualDate IS NULL 
    THEN 0 ELSE 1 END AS CompleteFlag, Year
FROM MILESTONE
WHERE ((Year = '2003/2004'))

GO
