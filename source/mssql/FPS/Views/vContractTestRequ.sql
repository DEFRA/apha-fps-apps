USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vContractTestRequ    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[vContractTestRequ] AS
SELECT	Buyer as JobCode,
	TestCode,
	NoRequired as NoTests,
	UnitPrice as TestPrice,
	DateCreated,
	ProjectBuyerCode
FROM	tlkpTestReqmt
WHERE tlkpTestReqmt.Buyer IN
	(SELECT vContractProject.ParentProject FROM vContractProject)

WITH CHECK OPTION

GO
