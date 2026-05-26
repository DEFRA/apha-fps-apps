USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtblTestRequ_TM    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtblTestRequ_TM    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblTestRequ_TM] AS
SELECT Buyer as JobCode,
	TestCode,
	NoRequired as NoTests,
	UnitPrice as TestPrice
FROM tlkpTestReqmt

GO
