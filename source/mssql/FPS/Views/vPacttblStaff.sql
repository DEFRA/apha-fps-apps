USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Searching for concatination ******/

CREATE VIEW [dbo].[vPacttblStaff] AS
SELECT tblWGEmployee.PACTid, tblEmployee.SPNumber, ISNULL(tblEmployee.Lastname,'') + ', ' + ISNULL(tblEmployee.firstname,'') AS Name, 	tblWGEmployee.WorkGroupGrade, tblEmployee.Title,  tblWGEmployee.PersonStatus, tblWGEmployee.PersonClass, 	tblWGEmployee.HrsPaid, tblWGEmployee.Leave, tblWGEmployee.SickSpecial, tblWGEmployee.HrsAvail
	FROM tblEmployee INNER JOIN tblWGEmployee ON tblEmployee.SPNumber = tblWGEmployee.SPNumber

GO
