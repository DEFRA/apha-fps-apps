USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_TimeByCategory    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_TimeByCategory    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procedure [dbo].[sp_TimeByCategory] as
SELECT WorkGroup.ProfitCentre, TimeCostCalcs.Division, TimeCostCalcs.WorkGroup,
	tblPeriod.PeriodName, Sum(TimeCostCalcs.Time) AS TotalTime
FROM tblPeriod, tlkpProject
join (TimeCostCalcs Join TimeCodeValid on (TimeCostCalcs.JobCode = TimeCodeValid.TimeCode) AND
	(TimeCostCalcs.WorkGroup = TimeCodeValid.WorkGroup)) 
	Join Workgroup ON TimeCostCalcs.WorkGroup = WorkGroup.WorkGroup
	ON tlkpProject.ParentProject = TimeCodeValid.ParentProject  
Where exists (SELECT Max(MonthNo) AS MaxOfMonthNo
	FROM ProjectMonthFinal
	WHERE CumFlag=1 and MonthNo = tblPeriod.EndPeriod)
 
GROUP BY WorkGroup.ProfitCentre, TimeCostCalcs.Division, TimeCostCalcs.WorkGroup,
 tblPeriod.PeriodName

GO
