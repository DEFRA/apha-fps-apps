USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vStaffJobHours]
AS
SELECT dbo.tblStaffJob.StaffID, SUM(dbo.tblStaffJob.plannedhours) 
    AS plannedhours
FROM dbo.tblStaffJob INNER JOIN
    dbo.tlkpProject ON 
    dbo.tblStaffJob.Jobcode = dbo.tlkpProject.ParentProject INNER JOIN
    dbo.tlkpProgram ON 
    dbo.tlkpProject.Program = dbo.tlkpProgram.ProgramNo
WHERE (dbo.tlkpProject.Program <> 'zt_prog') AND 
    (dbo.tlkpProgram.SECTOR_NAME = 'Charge')
GROUP BY dbo.tblStaffJob.StaffID

GO
