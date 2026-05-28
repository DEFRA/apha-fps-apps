USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.PandoraTimeCostCalcs    Script Date: 3/4/00 1:48:15 PM ******/
/****** Object:  View dbo.PandoraTimeCostCalcs    Script Date: 1/12/99 12:13:46 PM ******/
CREATE VIEW [dbo].[PandoraTimeCostCalcs] AS
SELECT * 
FROM TimeCostCalcs
WHERE  	TimeCostcalcs.Workgroup IN (SELECT tblUser_Workgroup.Workgroup FROM tblUser_Workgroup 

	WHERE tblUser_Workgroup.User_ID IN (SELECT tblUsers.User_ID FROM tblUsers 
	WHERE tblUsers.DT2UserName = USER_NAME()))

GO
