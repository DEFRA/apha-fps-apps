USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_tblAdditionalCosts_FPS_1998] (@cFPSVersion as VarChar(10), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tblAdditionalCosts(	[Year]  ,
	[JobCode] ,
	[Account],
	[Description] ,
	[ItemCost] )
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[JobCode] ,
	[Account],
	[Description] ,
	[ItemCost]
FROM  '+ @cFPSVersion + '.dbo.tblAdditionalCosts'
Exec(@sqlstr)

GO
