USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_CreateProjectMonthCasework    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.sp_CreateProjectMonthCasework    Script Date: 6/24/99 4:11:48 PM ******/
CREATE procEDURE [dbo].[sp_CreateProjectMonthCasework] AS
INSERT ProjectMonthCasework
SELECT DISTINCT qryProjectMonthCW.Project, 
	qryProjectMonthCW.MonthNo,
	qryProjectMonthCW.CWDebit,
	qryProjectMonthCW.CWCredit  

FROM qryProjectMonthCW

GO
