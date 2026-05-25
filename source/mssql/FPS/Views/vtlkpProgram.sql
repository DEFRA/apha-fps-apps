USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vtlkpProgram    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtlkpProgram    Script Date: 1/12/99 12:13:46 PM ******/
CREATE VIEW [dbo].[vtlkpProgram] AS
       
SELECT	tlkpProgram.ProgramNo, 
	tlkpProgram.ProgramName, 
	tlkpProgram.Directorate, 
	tlkpProgram.MINIM, 
	tlkpProgram.SECTOR_NAME, 
	tlkpProgram.CUSTOMER, 
	tlkpProgram.Target, 
	tlkpProgram.Manager
FROM	tlkpProgram
WHERE 	tlkpProgram.ProgramNo IN (SELECT tblUser_Program.ProgramNo 
	FROM tblUser_Program WHERE tblUser_Program.User_ID IN 
	(SELECT tblUsers.User_ID FROM tblUsers WHERE tblUsers.DT2UserName =  USER_NAME()))

GO
