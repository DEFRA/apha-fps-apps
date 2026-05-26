USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vWorkGroupRateHours]
AS
SELECT DISTINCT TimeCostCalcs_1.WorkGroup, dbo.TimeCostCalcs.ChargeRate AS Rate, dbo.tblMTConversion.Hours
FROM         dbo.TimeCostCalcs INNER JOIN
                      dbo.TimeCodeValid ON dbo.TimeCostCalcs.WorkGroup = dbo.TimeCodeValid.WorkGroup INNER JOIN
                      dbo.TimeCostCalcs AS TimeCostCalcs_1 ON dbo.TimeCodeValid.ParentProject = TimeCostCalcs_1.Project INNER JOIN
                      dbo.tblMTConversion ON TimeCostCalcs_1.Project = dbo.tblMTConversion.NewProject
WHERE     (dbo.TimeCodeValid.ParentProject = 'TG0100') AND (dbo.TimeCostCalcs.StaffID = '4464')

GO
