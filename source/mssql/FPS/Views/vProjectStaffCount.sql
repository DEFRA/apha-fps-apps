USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vProjectStaffCount    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[vProjectStaffCount] AS
SELECT tblStaffJob.JobCode, Count(StaffID) As CountOfStaff
From tblStaffJob
GROUP BY tblStaffJob.JobCode

GO
