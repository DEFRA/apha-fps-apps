USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_MonthlyOutput_PAct1998] (@cFPSVersion as VarChar(10), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_MonthlyOutput(	Year,[TestCode] ,
	[Buyer] ,
	[Month] ,
	[WorkGroup] ,
	[Volume] )
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[TestCode] ,
	[Buyer] ,
	[Month] ,
	[WorkGroup] ,
	[Volume] 


FROM  '+ @cFPSVersion + '.dbo.MonthlyOutput'
Exec(@sqlstr)

GO
