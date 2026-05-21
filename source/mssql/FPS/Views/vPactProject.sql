USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vPactProject]
AS
SELECT     ParentProject, ProjectTitle, Program, Customer, TransferIncome, Budget_CVL, PVSIncome, CustIncome AS Budget_Ext, FECcost AS ForecastCost, 
                      WIP_EOY, WIP_Limit, WIP_Current, Manager, ProjectStatus, ProjectParent, Contract, Disease, Finished, Comments, IsDefraProject, CostCentre, 
                      OracleProjectCode, SubAccountCode, ProjectGroup
FROM         dbo.tlkpProject


GO
