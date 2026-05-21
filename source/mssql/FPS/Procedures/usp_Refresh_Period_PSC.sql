USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_PSC]
	@period int
as
delete from [dbo].Period_Proj_SubContract
where period=@period
INSERT INTO [dbo].[Period_Proj_SubContract]
		([Period]
      ,[SubContCounter]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[Amount]
      ,[AcctCode])
Select @period,
     dbo.Proj_SubContract.SubContCounter, 
	dbo.Proj_SubContract.Project, 
	dbo.tlkpProject.OracleProjectCode, 
    dbo.tlkpProject.SubAccountCode, 
	CASE tlkpProject.IsDefraProject WHEN 0 THEN 'No' ELSE 'Yes' END AS IsDefraProject, 
	dbo.CostCentre.ProfitCentre AS OPC, 
    dbo.CostCentre.CostCentre AS OCC, 
	dbo.Proj_SubContract.Month, 
	dbo.Proj_SubContract.Amount, 
	dbo.Proj_SubContract.AcctCode

FROM         dbo.CostCentre RIGHT OUTER JOIN
                      dbo.tlkpProject ON dbo.CostCentre.CostCentre = dbo.tlkpProject.CostCentre INNER JOIN
                      dbo.Proj_SubContract ON dbo.tlkpProject.ParentProject = dbo.Proj_SubContract.Project

GO
