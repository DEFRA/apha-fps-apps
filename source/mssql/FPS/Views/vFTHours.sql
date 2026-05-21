USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vFTHours]
AS
SELECT     1.0 *
                          (SELECT     Setting
                            FROM          dbo.tblSettings
                            WHERE      (ID = 'HoursInDay')) AS FTHoursPerDay, 5.0 *
                          (SELECT     Setting
                            FROM          dbo.tblSettings AS tblSettings_1
                            WHERE      (ID = 'HoursInDay')) AS FTHoursPerWeek,
                          (SELECT     SUM(CVLHours) AS FTHoursPaid
                            FROM          dbo.tlkpMonthHours
                            WHERE      (NOT (Year =
                                                       (SELECT     RIGHT(DB_Var_Value, 4) AS Expr1
                                                         FROM          dbo.tblDB_Variables
                                                         WHERE      (DB_Var_Name = 'DB_Name')))) OR
                                                   (NOT (Month < 4))) AS FTHoursPaidPerYear

GO
