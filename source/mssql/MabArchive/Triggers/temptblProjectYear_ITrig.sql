USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblProjectYear_ITrig] ON [dbo].[temptblProjectYear] FOR INSERT AS
SET NOCOUNT ON
/* * PREVENT INSERTS IF NO MATCHING KEY IN 'temptblProject' */
IF (SELECT COUNT(*) FROM inserted) !=
   (SELECT COUNT(*) FROM temptblProject, inserted WHERE (temptblProject.Project = inserted.Project))
    BEGIN
        RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''temptblProject''.',16,1)
        ROLLBACK TRANSACTION
    END


GO
