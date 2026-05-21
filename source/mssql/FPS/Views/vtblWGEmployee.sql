USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblWGEmployee]
WITH  VIEW_METADATA
AS
SELECT     PACTid, SPNumber, WorkGroupGrade, PersonStatus, PersonClass, HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable, TimeRecorder, StartDate, 
                      EndDate, HoursPerWeek
FROM         dbo.tblWGEmployee
WHERE     (WorkGroupGrade IN
                          (SELECT     WGGrade
                            FROM          dbo.vWorkGroupGrade))
WITH CHECK OPTION

GO
