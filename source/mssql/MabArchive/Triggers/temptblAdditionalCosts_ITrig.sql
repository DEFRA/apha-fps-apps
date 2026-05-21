USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblAdditionalCosts_ITrig] ON [dbo].[temptblAdditionalCosts] FOR INSERT AS
SET NOCOUNT ON
/* * PREVENT INSERTS IF NO MATCHING KEY IN 'temptblProjectYear' */
IF (SELECT COUNT(*) FROM inserted) !=
   (SELECT COUNT(*) FROM temptblProjectYear, inserted WHERE (temptblProjectYear.Project = inserted.Project AND temptblProjectYear.YearNo = inserted.Year))
    BEGIN
        RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''temptblProjectYear''.',16,1)
        ROLLBACK TRANSACTION
    END


GO
