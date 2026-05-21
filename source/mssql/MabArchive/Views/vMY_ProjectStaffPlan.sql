USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMY_ProjectStaffPlan]
AS
SELECT     dbo.MY_tlkpProject.Year, dbo.MY_tlkpProject.ParentProject, dbo.MY_ProfitCentreGrade.PCGrade, dbo.MY_Staff.WorkGroupGrade, dbo.MY_Staff.Name,
                       dbo.MY_tblStaffJob.plannedhours, CASE WHEN (dbo.MY_tlkpProject.IsDefraProject <> 0 AND dbo.MY_tlkpProject.Year >= 2013) 
                      THEN my_profitcentregrade.npr + my_profitcentregrade.payrate ELSE my_profitcentregrade.chargerate END AS Rate, 
                      CASE WHEN (dbo.MY_tlkpProject.IsDefraProject <> 0 AND dbo.MY_tlkpProject.Year >= 2013) 
                      THEN plannedhours * (my_profitcentregrade.npr + my_profitcentregrade.payrate) 
                      ELSE plannedhours * my_profitcentregrade.chargerate END AS Cost
FROM         dbo.MY_tlkpProject INNER JOIN
                      dbo.MY_tblStaffJob ON dbo.MY_tlkpProject.Year = dbo.MY_tblStaffJob.Year AND 
                      dbo.MY_tlkpProject.ParentProject = dbo.MY_tblStaffJob.Jobcode INNER JOIN
                      dbo.MY_Staff ON dbo.MY_tblStaffJob.Year = dbo.MY_Staff.Year AND dbo.MY_tblStaffJob.StaffID = dbo.MY_Staff.StaffID INNER JOIN
                      dbo.MY_WorkGroupGrade ON dbo.MY_Staff.Year = dbo.MY_WorkGroupGrade.Year AND 
                      dbo.MY_Staff.WorkGroupGrade = dbo.MY_WorkGroupGrade.WGGrade INNER JOIN
                      dbo.MY_ProfitCentreGrade ON dbo.MY_WorkGroupGrade.Year = dbo.MY_ProfitCentreGrade.Year AND 
                      dbo.MY_WorkGroupGrade.ProfitCentreGrade = dbo.MY_ProfitCentreGrade.PCGrade


GO
