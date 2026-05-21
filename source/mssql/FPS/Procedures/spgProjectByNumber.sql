USE [FPS2025]
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
CREATE procEDURE [dbo].[spgProjectByNumber]
	@ProjectCode varchar(20)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT([ParentProject]) AS Total
	FROM [dbo].[tlkpProject]
	WHERE [ParentProject] = @ProjectCode
END

GO
