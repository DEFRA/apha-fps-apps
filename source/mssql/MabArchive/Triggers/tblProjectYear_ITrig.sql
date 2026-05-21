USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblProjectYear_ITrig] ON [dbo].[tblProjectYear] FOR INSERT AS
SET NOCOUNT ON
/* * PREVENT INSERTS IF NO MATCHING KEY IN 'tblProject' */
IF (SELECT COUNT(*) FROM inserted) !=
   (SELECT COUNT(*) FROM tblProject, inserted WHERE (tblProject.Project = inserted.Project))
    BEGIN
        RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''tblProject''.',16,1)
        ROLLBACK TRANSACTION
    END


GO
