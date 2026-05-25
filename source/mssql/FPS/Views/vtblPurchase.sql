USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vtblPurchase    Script Date: 3/4/00 1:48:19 PM ******/
/****** Object:  View dbo.vtblPurchase    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vtblPurchase] AS
SELECT	WorkGroup,
	Account,
	ItemDescription,
	Amount
FROM	tblPurchase
WHERE	(tblPurchase.WorkGroup IN 
	(SELECT tblBid.WorkGroup FROM tblBid 
	WHERE tblBid.WorkGroup IN
		(SELECT WorkGroup.WorkGroup FROM WorkGroup 
		WHERE WorkGroup.ProfitCentre IN 
			(SELECT tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre
			WHERE tblkpProfitCentre.ProfitCentre IN
				(SELECT tblUser_ProfitCentre.ProfitCentre FROM tblUser_ProfitCentre 
				WHERE tblUser_ProfitCentre.User_ID IN
					(SELECT tblUsers.User_ID FROM tblUsers 
					WHERE tblUsers.DT2UserName = USER_NAME()))))))
	
	AND 
	(tblPurchase.Account IN 
	(SELECT tblBid.Account FROM tblBid 
	WHERE tblBid.WorkGroup IN
		(SELECT WorkGroup.WorkGroup FROM WorkGroup 
		WHERE WorkGroup.ProfitCentre IN 
			(SELECT tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre
			WHERE tblkpProfitCentre.ProfitCentre IN
				(SELECT tblUser_ProfitCentre.ProfitCentre FROM tblUser_ProfitCentre 
				WHERE tblUser_ProfitCentre.User_ID IN
					(SELECT tblUsers.User_ID FROM tblUsers 
					WHERE tblUsers.DT2UserName = USER_NAME()))))))
	
WITH CHECK OPTION

GO
