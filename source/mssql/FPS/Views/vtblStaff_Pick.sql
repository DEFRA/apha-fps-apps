USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblStaff_Pick] AS
SELECT 	 tblWGEmployee.PactID as StaffID,
	ISNULL(tblEmployee.Lastname,'') + ', ' + ISNULL(tblEmployee.firstname,'') as Name,
	tblWGEmployee.WorkGroupGrade
	 
FROM	tblWGEmployee, tblEmployee
WHERE   tblWGEmployee.SPNumber = tblEmployee.SPNumber AND tblWGEmployee.MakeAvailable = -1
--order by ISNULL(tblEmployee.Lastname,'') + ', ' + ISNULL(tblEmployee.firstname,'')

GO
