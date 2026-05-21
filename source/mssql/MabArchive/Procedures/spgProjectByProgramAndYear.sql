USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Vincent Adcock
-- Create date: 25/06/2012
-- Description:	Returns a list of projects filtered by program and year
-- =============================================
CREATE PROCEDURE [dbo].[spgProjectByProgramAndYear]
	@Program varchar(10),
	@Year int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT [ParentProject] AS [Name], CASE [IsDefraProject] WHEN -1 THEN 1 ELSE 0 END AS [IsDefraProject]
	FROM [dbo].[vASUProjectList]
	WHERE [Program] = ISNULL(@Program, [Program])
	--AND [Year] >= @Year
	ORDER BY [ParentProject]
END


GO
