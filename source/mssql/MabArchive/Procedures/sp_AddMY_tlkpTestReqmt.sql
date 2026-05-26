USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run on DEFACPVWPSQL001 via loinked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_tlkpTestReqmt] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tlkpTestReqmt(
              year, 
	[TestCode] ,
	[Buyer] ,
	[UnitPrice],
	[NoRequired] ,
	[ProjectBuyerCode] ,
	[TestBuyerCode])
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[TestCode] ,
	[Buyer] ,
	[UnitPrice],
	[NoRequired] ,
	[ProjectBuyerCode] ,
	[TestBuyerCode]
FROM  '+ @cFPSVersion + '.dbo.tlkpTestReqmt'
Exec(@sqlstr)

GO
