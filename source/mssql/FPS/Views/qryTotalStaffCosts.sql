USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryTotalStaffCosts]
AS
SELECT DISTINCT ParentProject AS JobCode, SUM(Cost) AS TotalStaffCosts, SUM(PayCost) AS TotalPayCosts
FROM         dbo.vProjectStaffPlan
GROUP BY ParentProject

GO
