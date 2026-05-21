USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 04.10.24
--Updated to run on DEFACPVWPSQL001 via linked server to VLA88
--PL 07.10.24
--Updated to avoid using views in VLA88.FPSyyyy

CREATE procedure [dbo].[sp_AddMY_Staff] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(2000)
set @sqlstr ='INSERT INTO  MY_Staff(
		[Year]
      ,[StaffID]
      ,[Name]
      ,[WorkGroupGrade]
      ,[Title]
      ,[PersonStatus]
      ,[PersonClass]
      ,[HrsPaid]
      ,[Leave]
      ,[SickSpecial]
      ,[HrsAvail]

	)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +' as year, 

      WGE.PactID as [StaffID]
      ,ISNULL(E.Lastname,'''') + '', '' + ISNULL(E.firstname,'''') as Name
      ,WGE.[WorkGroupGrade]
      ,E.[Title]
      ,WGE.[PersonStatus]
      ,WGE.[PersonClass]
      ,WGE.[HrsPaid]
      ,WGE.[Leave]
      ,WGE.[SickSpecial]
      ,WGE.[HrsAvail]

FROM '+ @cFPSVersion + '.dbo.tblWGEmployee as WGE, '+ @cFPSVersion + '.dbo.tblEmployee as E
WHERE WGE.SPNumber = E.SPNumber AND
WGE.WorkGroupGrade IN 
(
	SELECT WGG.WGGrade from '+ @cFPSVersion + '.dbo.WorkgroupGrade AS WGG
	WHERE WGG.WorkGroup IN
	(
		SELECT WG.WorkGroup FROM '+ @cFPSVersion + '.dbo.WorkGroup AS WG
		WHERE WG.ProfitCentre IN
		(
			SELECT UPC.ProfitCentre FROM '+ @cFPSVersion + '.dbo.tblUser_ProfitCentre AS UPC
			WHERE UPC.[User_ID] IN
			(
				SELECT U.[User_ID] FROM '+ @cFPSVersion + '.dbo.tblUsers AS U WHERE U.UserName=USER_NAME(1)
			)
		)
	)
)'

exec(@sqlstr)


GO
