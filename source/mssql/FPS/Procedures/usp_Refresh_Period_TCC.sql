USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_TCC]
	@period int
as
delete from [dbo].[Period_TimeCostCalcs]
where period=@period

INSERT INTO .[dbo].[Period_TimeCostCalcs]
(
      [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[Month]
      ,[DefraProject]
      ,[OCC]
      ,[OPC]
      ,[SPC]
      ,[SCC]
      ,[Name]
      ,[GradeCode]
      ,[SPNumber]
      ,[ChargeRate]
      ,[Pay]
      ,[Nonpay]
      ,[Overhead]
      ,[Time]
      ,[TotalCost])

SELECT @Period, 
	tlkpProject.ParentProject AS Project,	
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	TimeCostCalcs.Month, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end AS DefraProject, 
	CostCentre.CostCentre AS OCC, 
	CostCentre.ProfitCentre AS OPC, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.CostCentre AS SCC, 
	TimeCostCalcs.Name, 
	TimeCostCalcs.GradeCode, 
	tblWGEmployee.SPNumber, 
	TimeCostCalcs.ChargeRate, 
	TimeCostCalcs.Pay, 
	TimeCostCalcs.Nonpay, 
	TimeCostCalcs.Overhead, 
	TimeCostCalcs.Time, 
	TimeCostCalcs.Cost AS TotalCost
FROM dbo.tblWGEmployee INNER JOIN ((tlkpProject LEFT JOIN CostCentre ON tlkpProject.CostCentre = CostCentre.CostCentre) INNER JOIN (TimeCostCalcs INNER JOIN WorkGroup ON TimeCostCalcs.WorkGroup = WorkGroup.WorkGroup) ON tlkpProject.ParentProject = TimeCostCalcs.Project) ON tblWGEmployee.PACTid = TimeCostCalcs.StaffID

GO
