USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vCustomerBudgetbyProgram]
AS
SELECT dbo.tlkpProject.Program, dbo.tlkpProject.ProjectStatus, dbo.tlkpProject.Customer, Sum(dbo.tlkpProject.Budget_CVL) AS CustomerBudget
FROM dbo.tlkpProject
GROUP BY dbo.tlkpProject.Program, dbo.tlkpProject.ProjectStatus, dbo.tlkpProject.Customer


GO
