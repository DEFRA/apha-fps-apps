USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger[dbo].[UITrig_tblAnimalReq] ON [dbo].[tblAnimalReq]
FOR INSERT, UPDATE
AS
INSERT AnimalReq_LOG 
	(

	JobCode,
	AnimalType,
	NumberOfDays,
	NumberOfAnimals,
	Date_Time,
	User_ID,
	Insert_Delete

)
(SELECT
	JobCode,
	AnimalType,
	NumberOfDays,
	NumberOfAnimals,
	GETDATE(),
	SYSTEM_USER,
	'I'
FROM INSERTED)


GO
