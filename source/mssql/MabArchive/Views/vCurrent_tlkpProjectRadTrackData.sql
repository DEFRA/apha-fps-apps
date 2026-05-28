USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vCurrent_tlkpProjectRadTrackData]
AS
SELECT     dbo.MY_tlkpProjectRadTrackData.Year, dbo.MY_tlkpProjectRadTrackData.Project, dbo.MY_tlkpProjectRadTrackData.BFBudget, 
                      dbo.MY_tlkpProjectRadTrackData.PYBudget, dbo.MY_tlkpProjectRadTrackData.Seedcorn, dbo.MY_tlkpProjectRadTrackData.ManHours, 
                      dbo.MY_tlkpProjectRadTrackData.ManDays, dbo.MY_tlkpProjectRadTrackData.ManYears, dbo.MY_tlkpProjectRadTrackData.PayCosts, 
                      dbo.MY_tlkpProjectRadTrackData.NonPayOHCosts, dbo.MY_tlkpProjectRadTrackData.TestCosts, dbo.MY_tlkpProjectRadTrackData.AnimalCosts, 
                      dbo.MY_tlkpProjectRadTrackData.NonAnimalCosts, dbo.MY_tlkpProjectRadTrackData.ManHoursChanged, 
                      dbo.MY_tlkpProjectRadTrackData.PayCostsChanged, dbo.MY_tlkpProjectRadTrackData.NonPayOHCostsChanged, 
                      dbo.MY_tlkpProjectRadTrackData.TestCostsChanged, dbo.MY_tlkpProjectRadTrackData.AnimalCostsChanged, 
                      dbo.MY_tlkpProjectRadTrackData.NonAnimalCostsChanged, dbo.MY_tlkpProjectRadTrackData.Adjustment, 
                      dbo.MY_tlkpProjectRadTrackData.AdjustmentComment, dbo.MY_tlkpProjectRadTrackData.Locked, dbo.MY_tlkpProjectRadTrackData.DateCosted, 
                      dbo.MY_tlkpProjectRadTrackData.CostedBy, dbo.MY_tlkpProjectRadTrackData.ActualExpenditure, dbo.MY_tlkpProjectRadTrackData.ActualManYears, 
                      dbo.MY_tlkpProjectRadTrackData.VLA_Budget
FROM         dbo.MY_tlkpProjectRadTrackData INNER JOIN
                      dbo.vLatestMonthYear ON dbo.MY_tlkpProjectRadTrackData.Year = dbo.vLatestMonthYear.Year

GO
