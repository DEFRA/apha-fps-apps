USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMY_ProjectAnimalPlan]
AS
SELECT     dbo.MY_tlkpProject.Year, dbo.MY_tlkpProject.ParentProject, dbo.MY_tblAnimalReq.AnimalType, dbo.MY_tblAnimalReq.NumberOfDays, 
                      dbo.MY_tblAnimalReq.NumberOfAnimals, CASE WHEN (isdefraproject <> 0 AND my_tlkpProject.year >= 2013) 
                      THEN defradailyrate ELSE dailyrate END AS Rate, CASE WHEN (isdefraproject <> 0 AND my_tlkpProject.year >= 2013) 
                      THEN defradailyrate ELSE dailyrate END * dbo.MY_tblAnimalReq.NumberOfDays * dbo.MY_tblAnimalReq.NumberOfAnimals AS cost
FROM         dbo.MY_tlkpProject INNER JOIN
                      dbo.MY_tblAnimalReq ON dbo.MY_tlkpProject.Year = dbo.MY_tblAnimalReq.Year AND 
                      dbo.MY_tlkpProject.ParentProject = dbo.MY_tblAnimalReq.JobCode INNER JOIN
                      dbo.MY_tblAnimals ON dbo.MY_tblAnimalReq.Year = dbo.MY_tblAnimals.Year AND 
                      dbo.MY_tblAnimalReq.AnimalType = dbo.MY_tblAnimals.AnimalType


GO
