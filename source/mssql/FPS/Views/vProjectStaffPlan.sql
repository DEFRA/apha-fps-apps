USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectStaffPlan]
AS
SELECT     dbo.tlkpProject.ParentProject, dbo.tlkpProgram.ProgramNo, dbo.tlkpProject.Contract, ISNULL(dbo.tblEmployee.LastName, '') 
                      + ', ' + ISNULL(dbo.tblEmployee.FirstName, '') AS Name, dbo.tblStaffJob.StaffID, dbo.tblStaffJob.plannedhours, 
                      CASE isdefraproject WHEN 0 THEN profitcentregrade.chargerate ELSE profitcentregrade.defrachargerate END AS ChargeRate, 
                      dbo.tblStaffJob.plannedhours * CASE sector_name WHEN 'charge' THEN 1 ELSE 0 END * CASE isdefraproject WHEN 0 THEN profitcentregrade.chargerate
                       ELSE profitcentregrade.defrachargerate END AS Cost, 
                      dbo.tblStaffJob.plannedhours * CASE sector_name WHEN 'charge' THEN 1 ELSE 0 END * dbo.ProfitCentreGrade.PayRate AS PayCost, 
                      dbo.ProfitCentreGrade.ProfitCentre, dbo.WorkGroupGrade.WorkGroup, dbo.WorkGroupGrade.WGGrade, dbo.ProfitCentreGrade.PCGrade, 
                      dbo.WorkGroupGrade.GradeCode
FROM         dbo.tblWGEmployee INNER JOIN
                      dbo.tblStaffJob ON dbo.tblWGEmployee.PACTid = dbo.tblStaffJob.StaffID INNER JOIN
                      dbo.tblEmployee ON dbo.tblWGEmployee.SPNumber = dbo.tblEmployee.SPNumber INNER JOIN
                      dbo.WorkGroupGrade ON dbo.tblWGEmployee.WorkGroupGrade = dbo.WorkGroupGrade.WGGrade INNER JOIN
                      dbo.ProfitCentreGrade ON dbo.WorkGroupGrade.ProfitCentreGrade = dbo.ProfitCentreGrade.PCGrade INNER JOIN
                      dbo.tlkpProject ON dbo.tblStaffJob.Jobcode = dbo.tlkpProject.ParentProject INNER JOIN
                      dbo.tlkpProgram ON dbo.tlkpProject.Program = dbo.tlkpProgram.ProgramNo

GO
