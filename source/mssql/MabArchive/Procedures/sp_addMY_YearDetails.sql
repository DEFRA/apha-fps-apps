USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updarted to run from DEFACPVWPSQL001 via linked server to VLA88
--

CREATE PROCEDURE [dbo].[sp_addMY_YearDetails] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  tlkpYear 
Select ' + Cast(@vcFPSYear as VarChar(4)) + ', [db_var_value] from ' + @cFPSVersion + '.dbo.tblDB_Variables
Where db_Var_name=''month'''
Exec(@sqlstr)

GO
