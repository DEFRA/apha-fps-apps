USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[tblkPeriodMonth]
AS
SELECT     dbo.tblPeriodMonth.EndMonth, dbo.tblPeriodMonth.MonthNo, dbo.tblPeriod.PeriodName
FROM         dbo.tblPeriod INNER JOIN
                      dbo.tblPeriodMonth ON dbo.tblPeriod.EndPeriod = dbo.tblPeriodMonth.EndMonth

GO
