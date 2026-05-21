USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qrySurvProjectSum    Script Date: 3/4/00 1:48:18 PM ******/
CREATE VIEW [dbo].[qrySurvProjectSum] AS
SELECT DISTINCT qryTCC_Union.Project, 
	qryTCC_Union.WorkGroup, 
	tlkpJobCode.Type, 
	Sum(TimeCostCalcs.Cost) AS HoursCost, 
	TimeCostCalcs.Month
FROM qryTCC_Union LEFT JOIN (tlkpJobCode RIGHT JOIN TimeCostCalcs ON (tlkpJobCode.ParentProject = TimeCostCalcs.Project) AND (tlkpJobCode.JobCode = TimeCostCalcs.JobCode)) ON (qryTCC_Union.Project = TimeCostCalcs.Project) AND (qryTCC_Union.Workgroup = TimeCostCalcs.WorkGroup)
GROUP BY qryTCC_Union.Project, qryTCC_Union.WorkGroup, tlkpJobCode.Type,TimeCostCalcs.Month

GO
