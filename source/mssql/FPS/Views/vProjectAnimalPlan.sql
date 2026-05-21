USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vProjectAnimalPlan]
AS
SELECT     dbo.tlkpProject.ParentProject, dbo.tlkpProject.Program, dbo.tblAnimalReq.AnimalType, dbo.tblAnimalReq.NumberOfDays, 
                      dbo.tblAnimalReq.NumberOfAnimals, CASE isdefraproject WHEN 0 THEN tblanimals.dailyrate ELSE tblanimals.defradailyrate END AS DailyRate, 
                      dbo.tblAnimalReq.NumberOfAnimals * dbo.tblAnimalReq.NumberOfDays * CASE isdefraproject WHEN 0 THEN tblanimals.dailyrate ELSE tblanimals.defradailyrate END
                       AS Cost, dbo.tblAnimals.Species, dbo.tblAnimals.Security_Level, dbo.tblAnimalReq.IndCounter
FROM         dbo.tlkpProject INNER JOIN
                      dbo.tblAnimalReq ON dbo.tlkpProject.ParentProject = dbo.tblAnimalReq.JobCode INNER JOIN
                      dbo.tblAnimals ON dbo.tblAnimalReq.AnimalType = dbo.tblAnimals.AnimalType

GO
