USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger [dbo].[tblPublicationProject_IU_trig] ON [dbo].[tblPublicationProject] 
FOR INSERT, UPDATE
AS



IF (Select ParentProject from inserted) Is Not Null
Begin
IF UPDATE(ParentProject)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM G_tlkpProject, inserted WHERE (G_tlkpProject.ParentProject = inserted.ParentProject))
            BEGIN
        		RAISERROR('This Project does not exist, please try another.', 16, 1)
                ROLLBACK TRANSACTION
            END
    END
end


GO
