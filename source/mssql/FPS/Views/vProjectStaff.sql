USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vProjectStaff    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[vProjectStaff] AS
SELECT DISTINCT tblStaffJob.Jobcode as Project, tblStaffJob.StaffID  
FROM tblStaffJob
UNION
SELECT DISTINCT MonthlyTime.ParentProject, MonthlyTime.PACTStaffID
FROM MonthlyTime

GO
