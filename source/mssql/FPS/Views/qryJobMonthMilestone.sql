USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryJobMonthMilestone]
AS
SELECT DISTINCT 
    Project, DueMonth, COUNT(MilestoneRef) AS MstoneDue, 
    SUM(CompleteFlag) AS Due__Done, SUM(OnTimeFlag) 
    AS OnTime
FROM qryMilestone1
GROUP BY Project, DueMonth

GO
