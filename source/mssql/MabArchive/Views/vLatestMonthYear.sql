USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.vLatestMonthYear
AS
SELECT     dbo.tlkpYear.Year, dbo.tlkpYear.LatestMonthReleased, CASE WHEN LatestMonthReleased = 1 THEN 'April ' + CAST(Year AS char(4)) 
                      WHEN LatestMonthReleased < 10 THEN 'April - ' + MonthName + ' ' + CAST(Year AS char(4)) ELSE 'April ' + CAST(Year AS char(4)) 
                      + ' - ' + MonthName + ' ' + CAST(Year + 1 AS char(4)) END AS Period
FROM         dbo.tlkpYear INNER JOIN
                      dbo.tlkpMonths ON dbo.tlkpYear.LatestMonthReleased = dbo.tlkpMonths.FMonthNo
WHERE     (dbo.tlkpYear.Year =
                          (SELECT     MAX(Year) AS Expr1
                            FROM          dbo.tlkpYear AS tlkpYear_1
                            WHERE      (LatestMonthReleased > 0)))

GO
