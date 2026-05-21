USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger[dbo].[MT_LOG_UTrig] ON [dbo].[MonthlyTime]
FOR UPDATE
AS
INSERT MT_LOG 
	([PactStaffID] ,
	[Timecode],
	[Month] ,
	[ParentProject],
	[WorkGroup] ,
	[Hours],
	[Date_Time] ,
	[User_ID],
	[Insert_Delete])
(SELECT
	[PactStaffID] ,
	[Timecode],
	[Month] ,
	[ParentProject],
	[WorkGroup] ,
	[Hours],
	GETDATE(),
	SYSTEM_USER,
	'UD'
FROM DELETED)
	

INSERT MT_LOG 
	([PactStaffID] ,
	[Timecode],
	[Month] ,
	[ParentProject],
	[WorkGroup] ,
	[Hours],
	[Date_Time] ,
	[User_ID],
	[Insert_Delete])
(SELECT
	[PactStaffID] ,
	[Timecode],
	[Month] ,
	[ParentProject],
	[WorkGroup] ,
	[Hours],
	GETDATE(),
	SYSTEM_USER,
	'UI'
FROM INSERTED)
	



GO
