USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vStaffUtilisation]
AS
SELECT     TOP (100) PERCENT dbo.vStaffUtilisation_Time.WorkGroup, dbo.tlkpMonthHours.Month, dbo.tlkpMonthHours.CVLHours AS FTHoursPerMonth, 
                      dbo.vStaffUtilisation_Time.FTHoursPerWeek, dbo.vStaffUtilisation_Time.Name, dbo.vStaffUtilisation_Time.StaffID, 
                      dbo.vStaffUtilisation_Time.GradeCode, dbo.vStaffUtilisation_Time.TimeRecorder, 
                      CAST(dbo.vStaffUtilisation_Time.HoursPerWeek * dbo.fnProRatapartMonth(dbo.vStaffUtilisation_Time.StartDate, dbo.vStaffUtilisation_Time.EndDate, 
                      dbo.tlkpMonthHours.Month, dbo.tlkpMonthHours.Year) AS decimal(9, 2)) AS HoursPerWeek, dbo.vStaffUtilisation_Time.HrsPaid, 
                      SUM(CASE project WHEN 'ZTLeave' THEN 0 WHEN 'ZTWork' THEN 0 ELSE time END) AS ChargedHours, 
                      SUM(CASE project WHEN 'ZTLeave' THEN time ELSE 0 END) AS ZTLeave, SUM(CASE project WHEN 'ZTWork' THEN time ELSE 0 END) 
                      AS ZTWork
FROM         dbo.tlkpMonthHours LEFT OUTER JOIN
                      dbo.vTimeCostCalcs_AllStaff ON dbo.tlkpMonthHours.FMonth = dbo.vTimeCostCalcs_AllStaff.Month RIGHT OUTER JOIN
                      dbo.vStaffUtilisation_Time ON dbo.vTimeCostCalcs_AllStaff.StaffID = dbo.vStaffUtilisation_Time.StaffID
GROUP BY dbo.vStaffUtilisation_Time.WorkGroup, dbo.tlkpMonthHours.Month, dbo.tlkpMonthHours.CVLHours, dbo.vStaffUtilisation_Time.Name, 
                      dbo.vStaffUtilisation_Time.GradeCode, dbo.vStaffUtilisation_Time.HrsPaid, dbo.vStaffUtilisation_Time.TimeRecorder, 
                      dbo.vStaffUtilisation_Time.FTHoursPerWeek, dbo.vStaffUtilisation_Time.StaffID, 
                      CAST(dbo.vStaffUtilisation_Time.HoursPerWeek * dbo.fnProRatapartMonth(dbo.vStaffUtilisation_Time.StartDate, dbo.vStaffUtilisation_Time.EndDate, 
                      dbo.tlkpMonthHours.Month, dbo.tlkpMonthHours.Year) AS decimal(9, 2))




GO
