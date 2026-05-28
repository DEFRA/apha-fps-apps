USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_ItblPurchase    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_ItblPurchase    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_ItblPurchase] 
	@WorkGroup varchar(50),
	@Account varchar(50),
	@ItemDescription varchar(50),
	@Amount money
AS
INSERT INTO vtblPurchase
VALUES (@WorkGroup, @Account, @ItemDescription, @Amount)

GO
