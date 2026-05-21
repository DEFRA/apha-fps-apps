USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vVRD_SplitLocation]
AS
SELECT     dbo.qVRD_SplitLocationMonthly.Location, SUM(dbo.qVRD_SplitLocationMonthly.LabLTSplitFee / dbo.qVRD_SplitMonthly.TotalLTSplitFee) 
                      AS SplitMultiplier
FROM         dbo.qVRD_SplitLocationMonthly INNER JOIN
                      dbo.qVRD_SplitMonthly ON dbo.qVRD_SplitLocationMonthly.Month = dbo.qVRD_SplitMonthly.Month
GROUP BY dbo.qVRD_SplitLocationMonthly.Location

GO
