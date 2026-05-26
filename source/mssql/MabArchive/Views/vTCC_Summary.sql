USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vTCC_Summary]
AS
SELECT Year, Project,Month,  SUM(Pay) AS Pay, SUM(NonPay) 
    AS NonPay, SUM(Overhead) AS Overhead
FROM dbo.MY_TimeCostCalcs
GROUP BY Year, Project,Month

GO
