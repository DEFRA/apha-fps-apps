USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tblkpProfitCentre_ITrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tblkpProfitCentre_ITrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tblkpProfitCentre_ITrig] on [dbo].[tblkpProfitCentre]
  for INSERT
  as
	Declare @ProfitCentre varchar(50)
	Declare @User_ID int
	SELECT @User_ID = (SELECT User_ID FROM tblUsers WHERE UserName = USER_NAME())
	SELECT @ProfitCentre = (SELECT ProfitCentre FROM inserted)
	
BEGIN
	INSERT INTO tblUser_ProfitCentre VALUES(@ProfitCentre, 42)
IF @User_ID <> 42 
BEGIN
	INSERT INTO tblUser_ProfitCentre
	VALUES( @ProfitCentre, @User_ID )
END
END


GO
