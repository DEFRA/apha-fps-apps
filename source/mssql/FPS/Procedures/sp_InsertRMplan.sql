USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_InsertRMplan    Script Date: 3/4/00 1:48:23 PM ******/
CREATE procEDURE [dbo].[sp_InsertRMplan] 
@@Project Varchar(20),
@@StaffID Varchar(50),
@@PlanHours Float
AS
INSERT
INTO tblStaffJob(StaffID, Jobcode, PlannedHours) 
Values (@@StaffID, @@Project,@@PlanHours)

GO
