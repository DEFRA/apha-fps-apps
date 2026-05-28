USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[fPeriodTests]
(@startperiod tinyint, @endPeriod tinyint, @project varchar(20))
returns table
as
return
(
SELECT 
 Project, OracleProjectCode, SubAccountCode, IsDefraProject DefraProject, OPC, OCC, Month, SPC, SCC, WorkGroup, TestCode, Sum(Volume) as Volume,  Sum(TestPrice) as TestPrice,  Sum(TotalCost) as TotalCost
FROM
(
SELECT Project AS Project, OracleProjectCode, SubAccountCode, IsDefraProject, OPC, OCC, Month, SPC, WorkGroup,  SCC,  TestCode, Volume,  TestPrice,  TotalCost
FROM Period_MonthlyOutput
WHERE
(Period_MonthlyOutput.period=@endPeriod)

Union All

SELECT Project, OracleProjectCode, SubAccountCode, IsDefraProject, OPC, OCC, Month, SPC, WorkGroup , SCC, TestCode, -Volume, - TestPrice,  TotalCost
FROM Period_MonthlyOutput
WHERE 
(Period_MonthlyOutput.period=@startperiod)
) as sq

Group By  Project, OracleProjectCode, SubAccountCode, IsDefraProject, OPC, OCC, Month, SPC, SCC, WorkGroup, TestCode
Having abs(sum(volume))>0
and project=Isnull(@project, project)
)

GO
