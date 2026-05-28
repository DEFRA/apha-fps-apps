USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_DtblTestRequ    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.usp_DtblTestRequ    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_DtblTestRequ] 
	@JobCode varchar(20),
	@TestCode varchar(20)
	
AS
DELETE FROM vtblTestRequ
WHERE	JobCode = @JobCode AND TestCode = @TestCode

GO
