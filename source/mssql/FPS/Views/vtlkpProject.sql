USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vtlkpProject]
WITH  VIEW_METADATA
AS
SELECT     ParentProject, ProjectTitle, Program, Customer, Manager, TransferIncome, CustIncome, WIP_EOY, WIP_Limit, WIP_Current, ProjectStatus, CostBookNo, DateCreated, 
                      FECcost, Profit, Budget_CVL, DateCosted, Disease, Contract, ProjectParent, ShortTitle, CaseworkSub, PVSIncome, PlanCaseworkDebit, Finished, OwningRC, 
                      Comments, CarryOver, CarryOverSeed, IsDefraProject, CostCentre, OracleProjectCode, SubAccountCode, ProjectGroup, IncomeAccountCode
FROM         dbo.tlkpProject
WHERE     (Program IN
                          (SELECT     ProgramNo
                            FROM          dbo.vtlkpProgram))
WITH CHECK OPTION


GO
