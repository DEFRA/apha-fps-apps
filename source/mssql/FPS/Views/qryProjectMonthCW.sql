USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryProjectMonthCW    Script Date: 3/4/00 1:48:16 PM ******/
CREATE VIEW [dbo].[qryProjectMonthCW] AS
SELECT DISTINCT ProjectMonth.Project, 
	ProjectMonth.MonthNo, 
	PlanCaseworkDebit/12 AS CWDebit, 
	TransferIncome*CaseworkSub/12 as CWCredit

FROM tlkpProject INNER JOIN ProjectMonth 
	ON tlkpProject.ParentProject = ProjectMonth.Project

GO
