USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vtlkpProjectGroup] AS
       
SELECT	ProjectGroup 
	
FROM	tlkpProjectGroup
WHERE 	ProjectGroup IN (SELECT tblUser_ProjectGroup.ProjectGroup
	FROM tblUser_ProjectGroup WHERE tblUser_ProjectGroup.User_ID IN 
	(SELECT tblUsers.User_ID FROM tblUsers WHERE tblUsers.DT2UserName =  USER_NAME()))


GO
