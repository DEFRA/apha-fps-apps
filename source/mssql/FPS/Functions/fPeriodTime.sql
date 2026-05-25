USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[fPeriodTime]
(@startperiod tinyint, @endPeriod tinyint, @project varchar(20))
returns table
as
Return
(
SELECT  Project, OracleProjectCode,  SubAccountCode, month, DefraProject,  OCC,  OPC,  SPC,  SCC, Name, GradeCode, SPNumber, ChargeRate, sum(Pay) as Pay, Sum(NonPay) as NonPay, Sum(overhead) as Overhead, sum(time) as Time,  sum(totalcost) as TotalCost

from(
SELECT Project, OracleProjectCode, SubAccountCode, Month, DefraProject, OCC, OPC, SPC, SCC, Name, GradeCode, SPNumber, ChargeRate, Pay, Nonpay, Overhead, Time, TotalCost
FROM Period_TimeCostCalcs
WHERE 

(Period_TimeCostCalcs.period=@endPeriod)

union all
SELECT Project, OracleProjectCode, SubAccountCode, Month,  DefraProject, OCC, OPC, SPC, SCC, Name, GradeCode, SPNumber, ChargeRate, -Pay, -Nonpay, -Overhead, -Time, -TotalCost
FROM Period_TimeCostCalcs
WHERE 

(Period_TimeCostCalcs.period=@startperiod)

) as sq

group by Project, OracleProjectCode,   SubAccountCode,  month, DefraProject,  OCC,  OPC,  SPC,  SCC, Name, GradeCode, SPNumber, ChargeRate
having abs(sum(time))>0.001 and Project not like 'ZT%'
and project=Isnull(@project, project)
)

GO
