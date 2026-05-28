USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.24
--Updated to run frm DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddMY_tblAnimalReq] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tblAnimalReq(
	[Year],	
	[JobCode] ,
	[AnimalType] ,
	[NumberOfDays] ,
	[NumberOfAnimals] )
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	[JobCode] ,
	[AnimalType] ,
	[NumberOfDays] ,
	[NumberOfAnimals] 
	 
FROM  '+ @cFPSVersion + '.dbo.tblAnimalReq'
Exec(@sqlstr)

GO
