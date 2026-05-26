USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vCurrent_ProjectInfo]
AS
SELECT     dbo.MY_tlkpProject.Year, dbo.MY_tlkpProject.ParentProject, dbo.G_tlkpProject.ProjectTitle, dbo.G_tlkpProject.CostBookNo, dbo.G_tlkpProject.Disease, 
                      dbo.G_tlkpProject.Contract, dbo.G_tlkpProject.ShortTitle, dbo.G_tlkpProject.ProjectStatus, dbo.MY_tlkpProject.Program, dbo.MY_tlkpProject.Customer, 
                      dbo.MY_tlkpProject.Manager, dbo.MY_tlkpProject.TransferIncome, dbo.MY_tlkpProject.CustIncome, dbo.MY_tlkpProject.WIP_EOY, 
                      dbo.MY_tlkpProject.WIP_Limit, dbo.MY_tlkpProject.WIP_Current, dbo.MY_tlkpProject.DateCreated, dbo.MY_tlkpProject.FECcost, 
                      dbo.MY_tlkpProject.Profit, dbo.MY_tlkpProject.Budget_CVL, dbo.MY_tlkpProject.CaseworkSub, dbo.MY_tlkpProject.PVSIncome, 
                      dbo.MY_tlkpProject.PlanCaseworkDebit, dbo.MY_tlkpProject.Source, dbo.MY_tlkpProjectRadTrackData.BFBudget, 
                      dbo.MY_tlkpProjectRadTrackData.PYBudget, dbo.MY_tlkpProjectRadTrackData.Seedcorn, dbo.MY_tlkpProjectRadTrackData.ManHours, 
                      dbo.MY_tlkpProjectRadTrackData.ManDays, dbo.MY_tlkpProjectRadTrackData.ManYears, dbo.MY_tlkpProjectRadTrackData.PayCosts, 
                      dbo.MY_tlkpProjectRadTrackData.NonPayOHCosts, dbo.MY_tlkpProjectRadTrackData.TestCosts, dbo.MY_tlkpProjectRadTrackData.AnimalCosts, 
                      dbo.MY_tlkpProjectRadTrackData.NonAnimalCosts, dbo.MY_tlkpProjectRadTrackData.ManHoursChanged, 
                      dbo.MY_tlkpProjectRadTrackData.PayCostsChanged, dbo.MY_tlkpProjectRadTrackData.NonPayOHCostsChanged, 
                      dbo.MY_tlkpProjectRadTrackData.TestCostsChanged, dbo.MY_tlkpProjectRadTrackData.AnimalCostsChanged, 
                      dbo.MY_tlkpProjectRadTrackData.NonAnimalCostsChanged, dbo.MY_tlkpProjectRadTrackData.Adjustment, 
                      dbo.MY_tlkpProjectRadTrackData.AdjustmentComment, dbo.MY_tlkpProjectRadTrackData.Locked, dbo.MY_tlkpProjectRadTrackData.DateCosted, 
                      dbo.MY_tlkpProjectRadTrackData.CostedBy, dbo.MY_tlkpProjectRadTrackData.ActualExpenditure, dbo.MY_tlkpProjectRadTrackData.ActualManYears, 
                      dbo.MY_tlkpProjectRadTrackData.VLA_Budget, dbo.vG_tlkpProjectIncome.TotalProjectValue, dbo.MY_tlkpProject.ProjectGroup
FROM         dbo.MY_tlkpProject INNER JOIN
                      dbo.vLatestMonthYear ON dbo.MY_tlkpProject.Year = dbo.vLatestMonthYear.Year INNER JOIN
                      dbo.G_tlkpProject ON dbo.MY_tlkpProject.ParentProject = dbo.G_tlkpProject.ParentProject INNER JOIN
                      dbo.vG_tlkpProjectIncome ON dbo.G_tlkpProject.ParentProject = dbo.vG_tlkpProjectIncome.Project LEFT OUTER JOIN
                      dbo.MY_tlkpProjectRadTrackData ON dbo.MY_tlkpProject.Year = dbo.MY_tlkpProjectRadTrackData.Year AND 
                      dbo.MY_tlkpProject.ParentProject = dbo.MY_tlkpProjectRadTrackData.Project

GO
