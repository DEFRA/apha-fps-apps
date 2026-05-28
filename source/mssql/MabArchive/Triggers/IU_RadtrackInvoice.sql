USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [dbo].[IU_RadtrackInvoice ] ON [dbo].[tblRadTrackInvoice] 
FOR INSERT, UPDATE
AS
if (Select count(*) from inserted where Contract is null and Project Is null )>0 
Begin
	RAISERROR('Project and Contract cannot both be Null', 16, 1)
        ROLLBACK TRANSACTION
End


GO
