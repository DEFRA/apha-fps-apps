USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_tlkpTestReqmt_FPS_1998] 
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tlkpTestReqmt(year, TestCode, 
 Buyer, 
 UnitPrice, 
 norequired, 
projectbuyercode,
testBuyercode,  source)
SELECT Year, 
	[TestCode] ,
	[Buyer] ,
	[UnitPrice],
	[NoRequired] ,
	[ProjectBuyerCode] ,
	[TestBuyerCode], [source]
FROM  _vPact_FPS_TestReq'
Exec(@sqlstr)

GO
