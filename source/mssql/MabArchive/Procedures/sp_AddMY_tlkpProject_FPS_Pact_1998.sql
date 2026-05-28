USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_tlkpProject_FPS_Pact_1998]
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  MY_tlkpProject(year, 
 parentproject,
program, 
 customer, 
 manager, 
transferincome, custincome, 
wip_eoy, 
wip_limit, 
wip_current, 
 projectstatus,
datecreated, 
feccost, 
profit, budget_cvl, 
 source)
SELECT  year, 
 parentproject,
program, 
 customer, 
 manager, 
transferincome, custincome, 
wip_eoy, 
wip_limit, 
wip_current, 
 projectstatus,
datecreated, 
feccost, 
profit, budget_cvl, 
 source
	 
FROM  dbo._vFPS_Pact_Projects'
Exec(@sqlstr)

GO
