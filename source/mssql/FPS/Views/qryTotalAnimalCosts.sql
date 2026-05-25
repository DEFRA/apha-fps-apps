USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryTotalAnimalCosts]
AS
SELECT DISTINCT ParentProject AS JobCode, SUM(Cost) AS TotalAnimalCosts
FROM         dbo.vProjectAnimalPlan
GROUP BY ParentProject

GO
