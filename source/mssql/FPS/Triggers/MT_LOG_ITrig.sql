USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE trigger[dbo].[MT_LOG_ITrig] ON [dbo].[MonthlyTime]
FOR INSERT
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
	'I'
FROM INSERTED)
	




GO
