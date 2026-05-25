USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vContractStaffJob    Script Date: 3/4/00 1:48:20 PM ******/
/****** Object:  View dbo.vContractStaffJob    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vContractStaffJob] AS
SELECT	*
FROM	tblStaffJob
WHERE	tblStaffJob.JobCode IN (SELECT vContractProject.ParentProject FROM vContractProject)
	
WITH CHECK OPTION

GO
