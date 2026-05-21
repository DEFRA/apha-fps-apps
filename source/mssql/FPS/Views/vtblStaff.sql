USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblStaff] AS
SELECT 	tblWGEmployee.PactID as StaffID,
	ISNULL(tblEmployee.Lastname,'') + ', ' + ISNULL(tblEmployee.firstname,'') as Name,
	tblWGEmployee.WorkGroupGrade,
	Title,
	PersonStatus,
	PersonClass,
	HrsPaid,
	Leave,
	SickSpecial,
	HrsAvail,
	MakeAvailable	
 
FROM	tblWGEmployee, tblEmployee
WHERE   tblWGEmployee.SPNumber = tblEmployee.SPNumber AND
	tblWGEmployee.WorkGroupGrade IN (SELECT vWorkGroupGrade.WGGrade FROM vWorkGroupGrade)

GO
