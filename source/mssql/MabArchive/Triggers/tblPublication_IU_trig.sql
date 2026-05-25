USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE trigger [dbo].[tblPublication_IU_trig] ON [dbo].[tblPublication] 
FOR INSERT, UPDATE 
AS

IF (Select program from inserted) Is Not Null
Begin
IF UPDATE(program)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tblRadTrackProg, inserted WHERE (tblRadTrackProg.program = inserted.program))
            BEGIN
        RAISERROR('This programme does not exist, please try another.', 16, 1)
                ROLLBACK TRANSACTION
            END
    END
end




GO
