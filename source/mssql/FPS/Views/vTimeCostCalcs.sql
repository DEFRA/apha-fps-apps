USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vTimeCostCalcs    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[vTimeCostCalcs] AS

SELECT	WorkGroup,
	JobCode,
	Project,
	Month,
	StaffID,
	GradeCode,
	Name,
	ChargeRate,
	Class,
	Time,
	Cost,
	Division,
	JobCodeOld

FROM	TimeCostCalcs INNER JOIN vtlkpProject
	ON TimeCostCalcs.Project = vtlkpProject.ParentProject

WITH CHECK OPTION

GO
