USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[UITrig_tblStaffJob] ON [dbo].[tblStaffJob]
FOR INSERT, UPDATE
AS
INSERT StaffJob_LOG 
	(
	StaffID,
	Jobcode,
	plannedhours,
	Date_Time,
	User_ID,
	Insert_Delete

)
(SELECT
	StaffID,
	Jobcode,
	plannedhours,
	GETDATE(),
	SYSTEM_USER,
	'I'
FROM INSERTED)


GO
