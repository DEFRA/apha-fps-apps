USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vtblkpProfitCentre    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtblkpProfitCentre    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblkpProfitCentre] AS
SELECT	ProfitCentre,
	ProfitCentreName,
	Division,
	CONTTARGET,
	ProfitCentreHead,
	DivisionID,
	Email_Recipient,
	HighLevelSummary
FROM	tblkpProfitcentre
WHERE 	tblkpProfitcentre.Profitcentre IN (SELECT tblUser_ProfitCentre.Profitcentre FROM tblUser_ProfitCentre 
	WHERE tblUser_ProfitCentre.User_ID IN (SELECT tblUsers.User_ID FROM tblUsers 
	WHERE tblUsers.DT2UserName = USER_NAME()))
WITH CHECK OPTION

GO
