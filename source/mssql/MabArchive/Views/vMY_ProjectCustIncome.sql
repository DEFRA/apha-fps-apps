USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMY_ProjectCustIncome]
AS
SELECT     COALESCE (PIMS.Year, FPS.Year) AS Year, COALESCE (PIMS.Project, FPS.ParentProject) AS Project, COALESCE (PIMS.PYBudget, FPS.CustIncome) 
                      AS CustInc
FROM         dbo.MY_FPSYearTotals AS FPS FULL OUTER JOIN
                      dbo.MY_tlkpProjectRadTrackData AS PIMS ON FPS.Year = PIMS.Year AND FPS.ParentProject = PIMS.Project

GO
