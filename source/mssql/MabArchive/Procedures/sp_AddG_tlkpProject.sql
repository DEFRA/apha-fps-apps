USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.2024
--Updated to run from DEFACPVWPSQL001 via linked server to VLA88
--

CREATE procedure [dbo].[sp_AddG_tlkpProject] (@cFPSVersion as varchar(20))
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  G_tlkpProject(
ParentProject, ProjectTitle, CostBookNo, Disease, Contract, 
    ShortTitle, ProjectStatus)
SELECT ParentProject, ProjectTitle, CostBookNo, Disease, Contract, 
    ShortTitle, ProjectStatus
FROM  '+ @cFPSVersion + '.dbo.tlkpProject
GROUP BY ParentProject, ProjectTitle, CostBookNo, Disease, 
    Contract, ShortTitle,ProjectStatus'
	 

Exec(@sqlstr)

GO
