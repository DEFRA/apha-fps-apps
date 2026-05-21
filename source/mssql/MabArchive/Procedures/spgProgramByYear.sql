USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Vincent Adcock
-- Create date: 25/06/2012
-- Description:	Returns a list of programs for the specified year
-- =============================================
CREATE PROCEDURE [dbo].[spgProgramByYear]
	@Year int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT [Program] AS [Name]
	FROM [dbo].[vASUProjectList]
	--WHERE [Year] >= @Year
	ORDER BY [Program]
END


GO
