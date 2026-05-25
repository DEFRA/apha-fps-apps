USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_MO]
	@period int
as
delete from [dbo].[Period_MonthlyOutput]
where period=@period
INSERT INTO [dbo].[Period_MonthlyOutput]
(	 [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[SPC]
      ,[WorkGroup]
      ,[SCC]
      ,[TestCode]
      ,[Volume]
      ,[TestPrice]
      ,[TotalCost]
)
SELECT  @Period,
	tlkpProject.ParentProject AS Project, 
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end IsDefraProject, 
	CostCentre.ProfitCentre AS OPC, 
	CostCentre.CostCentre AS OCC, 
	MonthlyOutput.Month, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.WorkGroup, 
	WorkGroup.CostCentre AS SCC,  
	MonthlyOutput.TestCode, 
	MonthlyOutput.Volume, 
	tlkpTestReqmt.UnitPrice as TestPrice, 
	convert(money,[UnitPrice]*[Volume]) AS TotalCost

FROM ((tlkpProject LEFT JOIN CostCentre 
	ON tlkpProject.CostCentre = CostCentre.CostCentre) 
	INNER JOIN (MonthlyOutput 
	INNER JOIN WorkGroup ON MonthlyOutput.WorkGroup = WorkGroup.WorkGroup) 
	ON tlkpProject.ParentProject = MonthlyOutput.Buyer) 
	INNER JOIN tlkpTestReqmt 
	ON (MonthlyOutput.Buyer = tlkpTestReqmt.projectBuyerCode) 
	AND (MonthlyOutput.TestCode = tlkpTestReqmt.TestCode)

GO
