USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_InsertMissingProjects] AS
DECLARE @Month int
DECLARE @Message varchar(10)
SELECT @month = 1
WHILE (@month < 13) 
BEGIN
	INSERT INTO ProjectMonth (Project, MonthNo)
	SELECT DISTINCT tlkpProject.ParentProject,
			@month AS MonthNo
	FROM 		tlkpProject LEFT JOIN ProjectMonth ON 
			tlkpProject.ParentProject = ProjectMonth.Project
			AND @month = ProjectMonth.MonthNo 
	WHERE ((ProjectMonth.Project IS NULL))
	ORDER BY ParentProject
	SELECT @month = @month + 1
	IF @month = 13
		BREAK
	ELSE
		CONTINUE
END

GO
