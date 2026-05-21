USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.2024
--Updated to run from DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_Proj_Invoice] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_Proj_Invoice
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	
	[ProjectParent] ,
	[Month] ,
	[Amount] ,
	[CostOfWork] ,
	[WIP] ,
	[ProfitLoss] ,
	[Detail] ,
	[InvoiceCounter] ,
	[Type] 

FROM  '+ @cFPSVersion + '.dbo.Proj_Invoice'
Exec(@sqlstr)

GO
