USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vStaffUtilisation_Summary]
AS
SELECT     WorkGroup, Month, SUM(ChargedHours) AS SumChargedHours, COUNT(DISTINCT StaffID) AS NoStaff, FTHoursPerMonth, SUM(HoursPerWeek) 
                      AS ActualWeekHoursAvailable, FTHoursPerWeek, SUM(ZTLeave) AS SumZTLeave
FROM         dbo.vStaffUtilisation
GROUP BY WorkGroup, Month, FTHoursPerMonth, FTHoursPerWeek

GO
