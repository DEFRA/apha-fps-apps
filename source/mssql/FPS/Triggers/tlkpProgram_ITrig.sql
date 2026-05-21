USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tlkpProgram_ITrig    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Trigger dbo.tlkpProgram_ITrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tlkpProgram_ITrig] on [dbo].[tlkpProgram]
  for INSERT
  as
	Declare @ProgramNo varchar(20)
	Declare @User_ID int
	SELECT @User_ID = (SELECT User_ID FROM tblUsers WHERE UserName = USER_NAME())
	SELECT @ProgramNo = (SELECT ProgramNo FROM inserted)
	
BEGIN
	INSERT INTO tblUser_Program VALUES(42, @ProgramNo)
IF @User_ID <> 42 
BEGIN
	INSERT INTO tblUser_Program
	VALUES( @User_ID, @ProgramNo)
END
END


GO
