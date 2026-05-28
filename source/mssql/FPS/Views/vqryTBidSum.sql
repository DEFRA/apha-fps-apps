USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vqryTBidSum    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vqryTBidSum    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vqryTBidSum] AS
SELECT	tblkpProfitCentre.ProfitCentre, 
	Sum(tblBid.GenBid) AS SumOfGenBid
FROM 	tblkpProfitCentre INNER JOIN 
	(WorkGroup INNER JOIN tblBid ON WorkGroup.WorkGroup = tblBid.WorkGroup)
	ON tblkpProfitCentre.ProfitCentre = WorkGroup.ProfitCentre
WHERE	tblkpProfitCentre.ProfitCentre IN (SELECT tblUser_ProfitCentre.ProfitCentre
	FROM tblUser_ProfitCentre WHERE tblUser_ProfitCentre.User_ID IN 
	(SELECT tblUsers.User_ID FROM tblUsers WHERE tblUsers.DT2UserName = USER_NAME()))
	
 
GROUP BY tblkpProfitCentre.ProfitCentre

GO
