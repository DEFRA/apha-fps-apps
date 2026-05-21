USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Vincent Adcock
-- Create date: 27/03/2013
-- Description:	Returns a list of programs for the specified year
-- =============================================
CREATE procEDURE [dbo].[spgProgramByYear]
	@Year int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT [ProgramNo] AS [Name]
	FROM [dbo].[tlkpProgram]
	--WHERE [Year] >= @Year
END

GO
