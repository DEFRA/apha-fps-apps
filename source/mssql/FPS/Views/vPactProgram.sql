USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPactProgram    Script Date: 3/4/00 1:48:15 PM ******/
/****** Object:  View dbo.vPactProgram    Script Date: 1/12/99 12:13:46 PM ******/
CREATE VIEW [dbo].[vPactProgram] AS
SELECT  tlkpProgram.ProgramNo, 
	tlkpProgram.ProgramName, 
	tlkpProgram.Directorate, 
	tlkpProgram.MINIM, 
	tlkpProgram.SECTOR_NAME, 
	tlkpProgram.CUSTOMER, 
	tlkpProgram.Manager AS leader
FROM tlkpProgram

GO
