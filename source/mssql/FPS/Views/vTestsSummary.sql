USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vTestsSummary] as

SELECT dbo.tlkpProgram.ProgramNo, dbo.tlkpProject.ParentProject, dbo.tlkpTestReqmt.TestCode, dbo.TestOrProduct.ItemDescription, [NoRequired]*[UnitPrice] AS [Planned Test Cost], dbo.tlkpTestReqmt.NoRequired AS [Planned Test Vol], Sum(dbo.MonthlyOutput.Volume) AS [Brought Test Volume], Sum([Volume]*[UnitPrice]) AS [Brought Test Cost]

FROM (((dbo.tlkpProgram INNER JOIN dbo.tlkpProject ON dbo.tlkpProgram.ProgramNo = dbo.tlkpProject.Program) INNER JOIN dbo.tlkpTestReqmt ON dbo.tlkpProject.ParentProject = dbo.tlkpTestReqmt.Buyer) LEFT JOIN dbo.MonthlyOutput ON (dbo.tlkpTestReqmt.Buyer = dbo.MonthlyOutput.Buyer) AND (dbo.tlkpTestReqmt.TestCode = dbo.MonthlyOutput.TestCode)) INNER JOIN dbo.TestOrProduct ON dbo.tlkpTestReqmt.TestCode = dbo.TestOrProduct.ItemCode

GROUP BY dbo.tlkpProgram.ProgramNo, dbo.tlkpProject.ParentProject, dbo.tlkpTestReqmt.TestCode, dbo.TestOrProduct.ItemDescription, [NoRequired]*[UnitPrice], dbo.tlkpTestReqmt.NoRequired

GO
