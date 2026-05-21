USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Amended to run on DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_WorkGroup] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_WorkGroup(
	   [Year]
      ,[WorkGroup]
      ,[ProfitCentre]
      ,[CostCentre]
      ,[Owner]
      ,[Description]
      ,[CentralOverhead]
      ,[SendEmail]
      ,[COS90]
      ,[CostCentreOld]
      ,[Email_Recipient]

	)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +' as year, 

       [WorkGroup]
      ,[ProfitCentre]
      ,[CostCentre]
      ,[Owner]
      ,[Description]
      ,[CentralOverhead]
      ,[SendEmail]
      ,[COS90]
      ,[CostCentreOld]
      ,[Email_Recipient]

	 
FROM  '+ @cFPSVersion + '.dbo.WorkGroup'

exec(@sqlstr)


GO
