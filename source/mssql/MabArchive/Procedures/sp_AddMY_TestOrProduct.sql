USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Amended to run on DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_TestOrProduct] (@cFPSVersion as varchar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_TestOrProduct
SELECT ' + Cast(@vcFPSYear as varChar(10)) +' 
      ,[ItemCode]
      ,[ItemDescription]
      ,[TestManager]
      ,[JobStatus]
      ,[UnitPriceVLA]
      ,[PriceAHVG]
      ,[Owner]
      ,[ChargeMethod]
      ,[ShortDescription]
      ,[DefraUnitPrice]
FROM  '+ @cFPSVersion + '.dbo.TestOrProduct'
Exec(@sqlstr)

GO
