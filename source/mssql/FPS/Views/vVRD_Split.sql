USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vVRD_Split]
AS
SELECT     TOP (100) PERCENT dbo.qVRD_SplitLocationMonthly.Location, 
                      SUM(dbo.qVRD_SplitLocationMonthly.LabLTSplitFee / dbo.qVRD_SplitMonthly.TotalLTSplitFee) AS Split
FROM         dbo.qVRD_SplitMonthly INNER JOIN
                      dbo.qVRD_SplitLocationMonthly ON dbo.qVRD_SplitMonthly.Month = dbo.qVRD_SplitLocationMonthly.Month
GROUP BY dbo.qVRD_SplitLocationMonthly.Location
ORDER BY dbo.qVRD_SplitLocationMonthly.Location

GO
