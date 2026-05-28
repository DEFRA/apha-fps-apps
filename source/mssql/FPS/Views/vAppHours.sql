USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vAppHours    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[vAppHours]
AS
SELECT     dbo.tblWGEmployee.WorkGroupGrade, SUM(dbo.tblStaffJob.plannedhours) AS SumOfplannedhours
FROM         dbo.tlkpProject INNER JOIN
                      dbo.tblStaffJob ON dbo.tlkpProject.ParentProject = dbo.tblStaffJob.Jobcode INNER JOIN
                      dbo.tblWGEmployee ON dbo.tblStaffJob.StaffID = dbo.tblWGEmployee.PACTid INNER JOIN
                      dbo.tlkpProgram ON dbo.tlkpProject.Program = dbo.tlkpProgram.ProgramNo
WHERE     (dbo.tlkpProject.Program <> 'ZT_Prog') AND (dbo.tlkpProject.ProjectStatus = 'approved') AND (dbo.tlkpProgram.SECTOR_NAME = 'Charge')
GROUP BY dbo.tblWGEmployee.WorkGroupGrade

GO
