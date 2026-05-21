USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE trigger[dbo].[tlkpProjectGroup_ITrig] on [dbo].[tlkpProjectGroup]
  for INSERT
  as
	Declare @Group varchar(50)
	Declare @User_ID int
	SELECT @User_ID = (SELECT User_ID FROM tblUsers WHERE UserName = USER_NAME())
	SELECT @Group = (SELECT ProjectGroup FROM inserted)
	
BEGIN
	INSERT INTO tblUser_ProjectGroup VALUES(42, @Group)
IF @User_ID <> 42 
BEGIN
	INSERT INTO tblUser_ProjectGroup
	VALUES( @User_ID, @Group)
END
END




GO
