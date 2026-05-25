USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vAnimalCosts]
AS
SELECT dbo.tblAnimalReq.NumberOfDays, 
    dbo.tblAnimalReq.NumberOfAnimals
FROM dbo.tblAnimals INNER JOIN
    dbo.tblAnimalReq ON 
    dbo.tblAnimals.AnimalType = dbo.tblAnimalReq.AnimalType

GO
