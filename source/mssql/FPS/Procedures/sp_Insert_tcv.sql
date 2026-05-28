USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_Insert_tcv    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_Insert_tcv    Script Date: 7/22/99 12:07:57 PM ******/
CREATE procEDURE [dbo].[sp_Insert_tcv]
	@OldCode Varchar(20),
	@NewCode VarChar(20)
AS
DECLARE @wg VARCHAR(50)
DECLARE @time VARCHAR(50)
DECLARE @pp VARCHAR(20)
DECLARE @test VARCHAR(50)
DECLARE @jc VARCHAR(50)
DECLARE @prt VARCHAR(20)
DECLARE @act BIT
DECLARE tcv_cursor INSENSITIVE CURSOR
FOR SELECT DISTINCT	
	tcv.WorkGroup, 
	(CASE  
		WHEN tcv.TimeCode = @OldCode THEN @NewCode 
		ELSE tcv.TimeCode
	END) AS TimeCode,
	(CASE 
		WHEN tcv.ParentProject = @OldCode THEN @NewCode 
		ELSE tcv.ParentProject
	END) AS parentProject,
	tcv.TestCode,
	(CASE 
		WHEN tcv.JobCode = @OldCode THEN @NewCode 
		ELSE tcv.JobCode
	END) AS JobCode,
	(CASE 
		WHEN tcv.Portfolio  = @OldCode THEN @NewCode 
		ELSE tcv.Portfolio
	END) AS Portfolio,
	tcv.Active
FROM TimeCodeValid tcv
WHERE tcv.Parentproject = @OldCode or tcv.Portfolio = @OldCode
OPEN tcv_cursor 
FETCH NEXT FROM tcv_cursor INTO @wg, @time, @pp, @test,@jc, @prt, @act
WHILE (@@FETCH_STATUS <> - 1)
BEGIN
	IF (@@FETCH_STATUS <> -2)
	BEGIN
		INSERT INTO TimeCodeValid (WorkGroup, TimeCode, ParentProject, TestCode, JobCode, Portfolio, Active)
		VALUES(@wg, @time, @pp, @test, @jc, @prt, @act)
	END
	FETCH NEXT FROM tcv_cursor INTO @wg, @time, @pp, @test,@jc, @prt, @act
END
DEALLOCATE tcv_cursor

GO
