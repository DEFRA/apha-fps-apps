USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_DeleteRMplan    Script Date: 3/4/00 1:48:23 PM ******/
CREATE procEDURE [dbo].[sp_DeleteRMplan] 
@@Project Varchar(20),
@@StaffID Varchar(50)
AS
Delete
FROM tblStaffJob 
WHERE StaffID = @@StaffID
AND Jobcode = @@Project

GO
