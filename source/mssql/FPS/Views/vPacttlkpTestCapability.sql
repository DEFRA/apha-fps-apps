USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vPacttlkpTestCapability] AS
SELECT  tlkpTestCapability.TestCode, tlkpTestCapability.WorkGroup, tlkpTestCapability.PlanPortfolio, tlkpTestCapability.SMScode, tlkpTestCapability.TestCode + tlkpTestCapability.Workgroup AS WGTestCode
FROM tlkpTestCapability

GO
