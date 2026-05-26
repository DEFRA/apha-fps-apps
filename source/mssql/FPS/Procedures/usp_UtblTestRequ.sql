USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_UtblTestRequ    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.usp_UtblTestRequ    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_UtblTestRequ] 
	@JobCode varchar(20),
	@TestCode_old varchar(20),
	@TestCode_new varchar(20),
	@NoTests float,
	@TestPrice money
AS
UPDATE	vtblTestRequ
SET	JobCode = @JobCode,
	TestCode = @TestCode_new,
	NoTests = @NoTests,
	TestPrice =@TestPrice,
	ProjectBuyerCode = @JobCode
WHERE	TestCode = @TestCode_old AND JobCode = @JobCode

GO
