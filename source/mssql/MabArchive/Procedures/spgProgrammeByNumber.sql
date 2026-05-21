USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Vincent Adcock
-- Create date: 28/05/2012
-- Description:	Returns one or more results matching the specified programm code
-- =============================================
CREATE PROCEDURE [dbo].[spgProgrammeByNumber]
	@ProgrammeCode varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT([ParentProject]) AS Total
	FROM [dbo].[vASUProjectList]
	WHERE [Program] = @ProgrammeCode
END


GO
