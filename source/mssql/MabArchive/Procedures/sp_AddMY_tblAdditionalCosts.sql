USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run from DEFACPVWPSQL001 voia linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_tblAdditionalCosts] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tblAdditionalCosts(	[Year]  ,
	[JobCode] ,
	[Account],
	[Description] ,
	[ItemCost] ,
	[Freq],
	[Supplier])
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[JobCode] ,
	[Account],
	[Description] ,
	[ItemCost] ,
	[Freq],
	[Supplier] 
FROM  '+ @cFPSVersion + '.dbo.tblAdditionalCosts'
Exec(@sqlstr)

GO
