USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_update_all_stats    Script Date: 3/4/00 1:48:20 PM ******/
/****** Object:  Stored Procedure dbo.sp_update_all_stats    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[sp_update_all_stats]
AS
/*
	This procedure will run UPDATE STATISTICS against
	all user-defined tables within this database.
*/
DECLARE @@tablename varchar(30)
DECLARE @@tablename_header varchar(75)
DECLARE tnames_cursor CURSOR FOR SELECT name FROM sysobjects 
	WHERE type = 'U'
OPEN tnames_cursor
FETCH NEXT FROM tnames_cursor INTO @@tablename
WHILE (@@fetch_status <> -1)
BEGIN
	IF (@@fetch_status <> -2)
	BEGIN
		SELECT @@tablename_header = "Updating " + 
			RTRIM(UPPER(@@tablename))
		PRINT @@tablename_header
		EXEC ("UPDATE STATISTICS " + @@tablename )
	END
	FETCH NEXT FROM tnames_cursor INTO @@tablename
END
PRINT " "
PRINT " "
SELECT @@tablename_header = "*************  NO MORE TABLES"
			+ "  *************" 
PRINT @@tablename_header
PRINT " "
PRINT "Statistics have been updated for all tables."
DEALLOCATE tnames_cursor

GO
