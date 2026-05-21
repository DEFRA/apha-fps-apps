USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vtblTestRequ    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtblTestRequ    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblTestRequ] AS
SELECT	Buyer as JobCode,
	TestCode,
	NoRequired as NoTests,
	UnitPrice as TestPrice,
	DateCreated,
	ProjectBuyerCode
FROM	tlkpTestReqmt

WHERE tlkpTestReqmt.Buyer IN
	(SELECT tlkpProject.ParentProject FROM tlkpProject
	WHERE tlkpProject.Program IN
		(SELECT tlkpProgram.ProgramNo FROM tlkpProgram
		WHERE tlkpProgram.ProgramNo IN
			(SELECT tblUser_Program.ProgramNo FROM tblUser_Program
			WHERE tblUser_Program.User_ID IN
				(SELECT tblUsers.User_ID FROM tblUsers 
				WHERE tblUsers.DT2UserName = USER_NAME()))))
WITH CHECK OPTION

GO
