USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_DWorkGroupGrade    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.usp_DWorkGroupGrade    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_DWorkGroupGrade] 
	@WGGrade varchar(50)
	
AS
DELETE FROM vWorkGroupGrade
WHERE	WGGrade = @WGGrade

GO
