USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[fPeriodExceptional]
(@startperiod tinyint, @endPeriod tinyint, @project varchar(20))
Returns table
as
Return
(
SELECT Project, OracleProjectCode, SubAccountCode, IsDefraProject DefraProject, OPC, OCC, Month, Sum(Amount) as TotalCost
FROM
(
SELECT Project, OracleProjectCode, SubAccountCode,  IsDefraProject, OPC, OCC, Month, Amount
FROM Period_Proj_SubContract
WHERE (((Period_Proj_SubContract.AcctCode) Not In ('LargeAnimals','SmallAnimals','Mice')))
and Period_Proj_SubContract.period=@endPeriod

UNION ALL

SELECT Project, OracleProjectCode, SubAccountCode, IsDefraProject, OPC, OCC, Month, -Amount
FROM Period_Proj_SubContract
WHERE (((Period_Proj_SubContract.AcctCode) Not In ('LargeAnimals','SmallAnimals','Mice')))
and Period_Proj_SubContract.period=@startperiod
) as sq
GROUP BY Project, OracleProjectCode, SubAccountCode, IsDefraProject, OPC, OCC, Month
HAVING abs(Sum(amount))>0.001
and Project=Isnull(@project, Project)
)

GO
