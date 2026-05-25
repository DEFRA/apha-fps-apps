USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vPlanTestCosts]
AS
SELECT dbo.tlkpTestReqmt.Buyer, Sum([unitPrice]*[norequired]) AS testPlancost
FROM dbo.tlkpTestReqmt
GROUP BY dbo.tlkpTestReqmt.Buyer



GO
