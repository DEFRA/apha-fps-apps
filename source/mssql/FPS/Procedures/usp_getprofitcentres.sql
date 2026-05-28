USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Stored Procedure dbo.usp_getprofitcentres    Script Date: 3/4/00 1:48:21 PM ******/
/****** Object:  Stored Procedure dbo.usp_getprofitcentres    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_getprofitcentres] AS
SELECT	ProfitCentre
FROM	tblUser_ProfitCentre
WHERE	User_ID IN (SELECT User_ID FROM tblUsers WHERE DT2UserName = USER_NAME())

GO
