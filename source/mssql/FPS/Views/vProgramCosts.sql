USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vProgramCosts]
AS
SELECT dbo.tlkpProgram.ProgramNo, Sum(dbo.ProjectMonthFinal.TotalCost) AS ProgramCost
FROM (dbo.tlkpProgram INNER JOIN dbo.tlkpProject ON dbo.tlkpProgram.ProgramNo = dbo.tlkpProject.Program) INNER JOIN dbo.ProjectMonthFinal ON dbo.tlkpProject.ParentProject = dbo.ProjectMonthFinal.Project
GROUP BY dbo.tlkpProgram.ProgramNo


GO
