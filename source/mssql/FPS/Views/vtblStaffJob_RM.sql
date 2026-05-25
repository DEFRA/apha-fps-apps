USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblStaffJob_RM]
AS
SELECT StaffID, Jobcode, plannedhours
FROM dbo.tblStaffJob
WHERE (StaffID IN
        (SELECT vtblWGEmployee.PactID
      FROM vtblWGEmployee))
WITH CHECK OPTION

GO
