USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_ItblTestRequ    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.usp_ItblTestRequ    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_ItblTestRequ] 
	@JobCode varchar(20),
	@TestCode varchar(20),
	@NoTests float,
	@TestPrice money
	
AS
INSERT INTO vtblTestRequ
VALUES (@JobCode,
	@TestCode,
	@NoTests,
	@TestPrice,
	GetDate(),
	@JobCode)

GO
