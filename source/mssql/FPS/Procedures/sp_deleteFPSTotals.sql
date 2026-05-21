USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_deleteFPSTotals    Script Date: 3/4/00 1:48:21 PM ******/
CREATE procEDURE [dbo].[sp_deleteFPSTotals] AS
DELETE from FPSYearTotals

GO
