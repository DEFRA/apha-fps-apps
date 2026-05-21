USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vQryBid    Script Date: 3/4/00 1:48:18 PM ******/
/****** Object:  View dbo.vQryBid    Script Date: 1/12/99 12:13:48 PM ******/
CREATE VIEW [dbo].[vQryBid] AS
SELECT DISTINCT tblkpAccountCategory.AccShortName, tblBid.WorkGroup, tblBid.GenBid, WorkGroup.ProfitCentre
FROM (tblkpAccountCategory LEFT JOIN tblBid ON tblkpAccountCategory.AccShortName = tblBid.Account) LEFT JOIN WorkGroup ON tblBid.WorkGroup = WorkGroup.WorkGroup
GROUP BY tblkpAccountCategory.AccShortName, tblBid.WorkGroup, tblBid.GenBid, WorkGroup.ProfitCentre

GO
