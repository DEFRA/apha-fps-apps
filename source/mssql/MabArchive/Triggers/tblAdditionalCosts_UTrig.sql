USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblAdditionalCosts_UTrig] ON [dbo].[tblAdditionalCosts] FOR UPDATE AS
SET NOCOUNT ON
/* * PREVENT UPDATES IF NO MATCHING KEY IN 'tblProjectYear' */
IF UPDATE(Project) OR UPDATE(Year)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tblProjectYear, inserted WHERE (tblProjectYear.Project = inserted.Project AND tblProjectYear.YearNo = inserted.Year))
            BEGIN
                RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''tblProjectYear''.',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
