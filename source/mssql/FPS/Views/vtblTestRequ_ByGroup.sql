USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View [dbo].[vtblTestRequ]    Script Date: 11/08/2012 13:27:03 ******/

/****** Object:  View dbo.vtblTestRequ    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtblTestRequ    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblTestRequ_ByGroup] AS
SELECT	Buyer as JobCode,
	TestCode,
	NoRequired as NoTests,
	UnitPrice as TestPrice,
	DateCreated,
	ProjectBuyerCode
FROM	tlkpTestReqmt WHERE tlkpTestReqmt.Buyer IN(SELECT ParentProject FROM vtlkpProject_ByGroup)
WITH CHECK OPTION

GO
