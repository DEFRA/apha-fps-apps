USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[DTrig_tblAdditionalCosts] ON [dbo].[tblAdditionalCosts]
FOR DELETE
AS
INSERT AdditionalCosts_LOG 
	(
	JobCode,
	Account,
	Description,
	ItemCost,
	Freq,
	Supplier,
	Date_Time,
	User_ID,
	Insert_Delete
)
(SELECT
	JobCode,
	Account,
	Description,
	ItemCost,
	Freq,
	Supplier,
	GETDATE(),
	SYSTEM_USER,
	'D'
FROM DELETED)


GO
