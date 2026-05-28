USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Stored Procedure dbo.usp_UtblPurchase    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_UtblPurchase    Script Date: 1/12/99 12:14:27 PM ******/
CREATE proc [dbo].[usp_UtblPurchase] 
	@WorkGroup varchar(50),
	@Account varchar(50),
	@ItemDescription_new varchar(50),	
	@ItemDescription_old varchar(50),
	@Amount money
AS
BEGIN
IF 	@WorkGroup IN 
	(SELECT tblBid.WorkGroup FROM tblBid 
	WHERE tblBid.WorkGroup IN
		(SELECT WorkGroup.WorkGroup FROM WorkGroup 
		WHERE WorkGroup.ProfitCentre IN 
			(SELECT tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre
			WHERE tblkpProfitCentre.ProfitCentre IN
				(SELECT tblUser_ProfitCentre.ProfitCentre FROM tblUser_ProfitCentre 
				WHERE tblUser_ProfitCentre.User_ID IN
					(SELECT tblUsers.User_ID FROM tblUsers 
					WHERE tblUsers.DT2UserName = USER_NAME())))))
UPDATE	tblPurchase
SET	ItemDescription = @ItemDescription_new,
	Amount = @Amount
WHERE	WorkGroup = @WorkGroup AND Account = @Account AND ItemDescription = @ItemDescription_old
END

GO
