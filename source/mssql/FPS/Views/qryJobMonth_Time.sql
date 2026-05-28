USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Time    Script Date: 3/4/00 1:48:15 PM *****
***** Object:  View dbo.qryJobMonth_Time    Script Date: 1/12/99 12:13:46 PM *****
***** Object:  View dbo.qryJobMonth_Time    Script Date: 10/27/98 11:55:27 AM ******/
CREATE VIEW [dbo].[qryJobMonth_Time]
AS
SELECT DISTINCT Project, Month, SUM(Cost) AS SumOfCost, SUM(Time) AS SumOfHours, SUM(Pay) AS SumOfPayRate
FROM         dbo.TimeCostCalcs
GROUP BY Project, Month

GO
