USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[upPM1Report]
/*
for FPS PostMortem reporting
Ilkka Salminen, October 2005

this should be run whenever tblPeriod is updated

*/
AS

exec upEmptyPM1Report	-- empty tblPostMortem1Report

exec upUpdatePM1Report	-- update tblPostMortem1Report

GO
