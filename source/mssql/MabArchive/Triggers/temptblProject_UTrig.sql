USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblProject_UTrig] ON [dbo].[temptblProject] FOR UPDATE AS
SET NOCOUNT ON
/* * PREVENT UPDATES IF DEPENDENT RECORDS IN 'temptblProjectYear' */
IF UPDATE(Project)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, temptblProjectYear WHERE (deleted.Project = temptblProjectYear.Project)) > 0
            BEGIN
                RAISERROR ( 'The record can''t be deleted or changed. Since related records exist in table ''temptblProjectYear'', referential integrity rules would be violated.',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
