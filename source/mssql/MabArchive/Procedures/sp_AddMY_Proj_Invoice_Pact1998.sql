USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_Proj_Invoice_Pact1998] (@cFPSVersion as VarChar(10), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_Proj_Invoice(year,	[ProjectParent] ,
	[Month] ,
	[Amount] ,
	[CostOfWork] ,
	[WIP] ,
	[ProfitLoss] ,
	[Detail] ,
	[InvoiceCounter] 
)
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	
	[ProjectParent] ,
	[Month] ,
	[Amount] ,
	[CostOfWork] ,
	[WIP] ,
	[ProfitLoss] ,
	[Detail] ,
	[InvoiceCounter] 


FROM  '+ @cFPSVersion + '.dbo.Proj_Invoice WHERE [ProjectParent] is not NULL '
Exec(@sqlstr)

GO
