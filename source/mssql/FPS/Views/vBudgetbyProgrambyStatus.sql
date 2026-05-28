USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vBudgetbyProgrambyStatus]
AS
SELECT dbo.tlkpProject.Program, dbo.tlkpProject.ProjectStatus, Sum(dbo.tlkpProject.Budget_CVL) AS StatusBudget
FROM dbo.tlkpProject
GROUP BY dbo.tlkpProject.Program, dbo.tlkpProject.ProjectStatus



GO
