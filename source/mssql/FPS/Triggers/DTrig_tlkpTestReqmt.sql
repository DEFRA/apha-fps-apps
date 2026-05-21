USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[DTrig_tlkpTestReqmt] ON [dbo].[tlkpTestReqmt]
FOR DELETE
AS
INSERT TestReq_LOG 
	(
	TestCode,
	Buyer,
	UnitPrice,
	NoRequired,
	Date_Time,
	User_ID,
	Insert_Delete

)
(SELECT
	TestCode,
	Buyer,
	UnitPrice,
	NoRequired,
	GETDATE(),
	SYSTEM_USER,
	'D'
FROM DELETED)


GO
