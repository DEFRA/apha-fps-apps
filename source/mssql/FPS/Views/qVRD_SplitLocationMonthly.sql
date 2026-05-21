USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qVRD_SplitLocationMonthly]
AS
SELECT     TOP (100) PERCENT Location, Month, SUM(LTSplitFee) AS LabLTSplitFee
FROM         dbo.vPostMort_VRD_Split
GROUP BY Location, Month
ORDER BY Location, Month

GO
