USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddG_tlkpProject_FPS_1998] (@cFPSVersion as varchar(10))
AS
declare @sqlstr as varchar(800)
set @sqlstr ='INSERT INTO  G_tlkpProject(
ParentProject, ProjectTitle, CostBookNo)   
    
SELECT ParentProject, ProjectTitle, CostBookNo   
    
FROM  '+ @cFPSVersion + '.dbo.tlkpProject
GROUP BY ParentProject, ProjectTitle, CostbookNo   '
     
	 

Exec(@sqlstr)

GO
