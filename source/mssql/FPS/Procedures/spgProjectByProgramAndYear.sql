USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Vincent Adcock
-- Create date: 27/03/2013
-- Description:	Returns a list of projects filtered by program and year
-- =============================================
CREATE procEDURE [dbo].[spgProjectByProgramAndYear]
	@Program varchar(10),
	@Year int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT [ParentProject] AS [Name], CASE [IsDefraProject] WHEN -1 THEN 1 ELSE 0 END AS [IsDefraProject]
	FROM [dbo].[tlkpProject]
	WHERE ProjectStatus <> 'Completed'
	AND [Program] = ISNULL(@Program, [Program])
	--AND [Year] >= @Year
END

GO
