USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vCurrentPeriod]
AS
SELECT     PeriodName
FROM         dbo.tblPeriod
WHERE     (EndPeriod =
                          (SELECT     MAX(EndPeriod) AS MaxEndPeriod
                            FROM          dbo.tblPeriod
                            GROUP BY FinalSummariesRun
                            HAVING      (FinalSummariesRun = - 1)))

GO
