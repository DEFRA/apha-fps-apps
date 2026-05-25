USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.All_Staff_Project    Script Date: 3/4/00 1:48:19 PM ******/
CREATE VIEW [dbo].[All_Staff_Project] AS
SELECT tblStaffJob.StaffID as PactID, tblStaffJob.JobCode as ParentProject
FROM tblStaffJob
UNION
SELECT MonthlyTime.PactStaffID, MonthlyTime.ParentProject
FROM MonthlyTime
GROUP BY MonthlyTime.PactStaffID, MonthlyTime.ParentProject

GO
