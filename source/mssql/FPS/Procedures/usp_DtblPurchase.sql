USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_DtblPurchase    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_DtblPurchase    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_DtblPurchase] 
	@WorkGroup varchar(50),
	@Account varchar(50),
	@ItemDescription varchar(50)
	
AS
DELETE FROM vtblPurchase
WHERE	WorkGroup = @WorkGroup AND Account = @Account AND ItemDescription = @ItemDescription

GO
