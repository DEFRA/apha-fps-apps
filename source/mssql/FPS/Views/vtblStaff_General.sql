USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblStaff_General] AS
SELECT 	tblWGEmployee.PactID as StaffID,
	ISNULL(tblEmployee.Lastname,'') + ', ' + ISNULL(tblEmployee.firstname,'') as Name,
	tblWGEmployee.WorkGroupGrade
	
 
FROM	tblWGEmployee, tblEmployee
WHERE   tblWGEmployee.SPNumber = tblEmployee.SPNumber

GO
