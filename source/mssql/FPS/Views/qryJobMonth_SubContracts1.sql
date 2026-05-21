USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryJobMonth_SubContracts1] as 
SELECT DISTINCT Proj_SubContract.Project, Proj_SubContract.Month, Proj_SubContract.AcctCode, 
Sum(Proj_SubContract.Amount) AS Total, 
Case 
	When acctcode IN('LargeAnimals','SmallAnimals', 'Mice') Then Sum(Proj_SubContract.Amount)
	Else 0
End AS Animals1, 
Case 
	When acctcode IN('LargeAnimals','SmallAnimals', 'Mice')  Then 0
	Else Sum(Proj_SubContract.Amount)
End AS Other1
FROM Proj_SubContract
GROUP BY Proj_SubContract.Project, Proj_SubContract.Month, Proj_SubContract.AcctCode

GO
