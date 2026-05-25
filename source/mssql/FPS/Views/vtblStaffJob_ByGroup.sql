USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View [dbo].[vtblStaffJob]    Script Date: 11/08/2012 13:27:02 ******/

/****** Object:  View dbo.vtblStaffJob    Script Date: 3/4/00 1:48:20 PM ******/
/****** Object:  View dbo.vtblStaffJob    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vtblStaffJob_ByGroup] AS 

SELECT	StaffID,
	Jobcode,
	plannedhours
FROM	tblStaffJob
WHERE	 tblStaffJob.JobCode IN (SELECT ParentProject FROM vtlkpProject_ByGroup)
WITH CHECK OPTION

GO
