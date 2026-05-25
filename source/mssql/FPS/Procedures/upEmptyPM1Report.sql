USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[upEmptyPM1Report]
/*
Ilkka Salminen, October 2005

empties tblPostMortem1Report

called by upPM1Report

for FPS PostMortem reporting

*/
AS
	delete from tblPostMortem1Report

GO
