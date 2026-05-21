USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vTestOrProduct_TM    Script Date: 3/4/00 1:48:15 PM ******/
CREATE VIEW [dbo].[xxTestOrProduct_TM] AS
SELECT * 
FROM TestOrProduct
WHERE TestOrProduct.Owner IN (SELECT tblUser_TestOwner.Test_Owner
	FROM tblUser_TestOwner WHERE tblUser_TestOwner.User_ID IN
	(SELECT tblUsers.User_ID FROM tblUsers WHERE tblUsers.UserName =  USER_NAME()))

GO
