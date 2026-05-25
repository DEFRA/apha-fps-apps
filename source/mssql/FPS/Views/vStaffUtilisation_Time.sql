USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vStaffUtilisation_Time]
AS
SELECT     dbo.tblWGEmployee.PACTid AS StaffID, ISNULL(dbo.tblEmployee.LastName, '') + ', ' + ISNULL(dbo.tblEmployee.FirstName, '') AS Name, 
                      dbo.WorkGroupGrade.WorkGroup, dbo.WorkGroupGrade.GradeCode, dbo.tblWGEmployee.WorkGroupGrade, dbo.tblEmployee.Title, 
                      dbo.tblWGEmployee.PersonStatus, dbo.tblWGEmployee.PersonClass, dbo.tblWGEmployee.HrsPaid, dbo.tblWGEmployee.TimeRecorder, 
                      dbo.tblWGEmployee.HoursPerWeek, dbo.vFTHours.FTHoursPerDay, dbo.vFTHours.FTHoursPerWeek, dbo.tblWGEmployee.StartDate, 
                      dbo.tblWGEmployee.EndDate
FROM         dbo.tblWGEmployee INNER JOIN
                      dbo.tblEmployee ON dbo.tblWGEmployee.SPNumber = dbo.tblEmployee.SPNumber INNER JOIN
                      dbo.WorkGroupGrade ON dbo.tblWGEmployee.WorkGroupGrade = dbo.WorkGroupGrade.WGGrade CROSS JOIN
                      dbo.vFTHours
WHERE     (dbo.tblEmployee.FirstName <> 'General')

GO
