USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[UITrig_tlkpTestReqmt] ON [dbo].[tlkpTestReqmt]
FOR INSERT, UPDATE
AS
INSERT TestReq_LOG 
	(
	TestCode,
	Buyer,
	UnitPrice,
	NoRequired,
	ProjectBuyerCode,
	TestBuyerCode,
	Active,
	Date_Time,
	User_ID,
	Insert_Delete

)
(SELECT
	TestCode,
	Buyer,
	UnitPrice,
	NoRequired,
	ProjectBuyerCode,
	TestBuyerCode,
	Active,
	GETDATE(),
	SYSTEM_USER,
	'I'
FROM INSERTED)


GO
