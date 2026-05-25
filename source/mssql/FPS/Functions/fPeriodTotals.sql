USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE function [dbo].[fPeriodTotals]
(@startperiod tinyint, @endPeriod tinyint, @project varchar(20))
returns table
as
return
(
Select Project, OracleProjectCode,  Sum(TotalCost) as TotalCost
From
(
SELECT Project, OracleProjectCode,  TotalCost
FROM fPeriodAnimals(@startperiod , @endPeriod, @project )
union all
SELECT Project, OracleProjectCode,  TotalCost
FROM fPeriodExceptional(@startperiod , @endPeriod, @project )
union all
SELECT Project, OracleProjectCode,  TotalCost
FROM fPeriodTime(@startperiod , @endPeriod,@project )
union all
SELECT Project, OracleProjectCode,  TotalCost
FROM fPeriodTests(@startperiod , @endPeriod, @project )
) as sq
GROUP BY  Project, OracleProjectCode
)

GO
