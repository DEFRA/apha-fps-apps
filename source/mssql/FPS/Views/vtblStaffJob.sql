USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtblStaffJob    Script Date: 3/4/00 1:48:20 PM ******/
/****** Object:  View dbo.vtblStaffJob    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vtblStaffJob] AS 
/* FOR PROGRAM MANAGERS ONLY */
SELECT	StaffID,
	Jobcode,
	plannedhours
FROM	tblStaffJob
WHERE	/* tblStaffJob.StaffID IN (SELECT vtblStaff.StaffID FROM vtblStaff)
	OR */ tblStaffJob.JobCode IN (SELECT vtlkpProject.ParentProject FROM vtlkpProject)
WITH CHECK OPTION

GO
