USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger[dbo].[MO_LOG_UTrig] ON [dbo].[MonthlyOutput] 
FOR UPDATE
AS
INSERT MO_LOG 
	([TestCode] ,
	[Buyer],
	[Month] ,
	[WorkGroup] ,
	[Volume] ,
	[WGBuyer] ,
	[Date_Time] ,
	[User_ID],	[Insert_Delete])
(SELECT
	[TestCode] ,
	[Buyer],
	[Month] ,
	[WorkGroup] ,
	[Volume] ,
	[WGBuyer],
	GETDATE(),
	SYSTEM_USER,
	'UD'
FROM DELETED)

	

INSERT MO_LOG 
	([TestCode] ,
	[Buyer],
	[Month] ,
	[WorkGroup] ,
	[Volume] ,
	[WGBuyer] ,
	[Date_Time] ,
	[User_ID],	[Insert_Delete])
(SELECT
	[TestCode] ,
	[Buyer],
	[Month] ,
	[WorkGroup] ,
	[Volume] ,
	[WGBuyer],
	GETDATE(),
	SYSTEM_USER,
	'UI'
FROM INSERTED)
	







GO
