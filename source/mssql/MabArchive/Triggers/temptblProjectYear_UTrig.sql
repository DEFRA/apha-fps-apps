USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblProjectYear_UTrig] ON [dbo].[temptblProjectYear] FOR UPDATE AS
SET NOCOUNT ON
/* * PREVENT UPDATES IF NO MATCHING KEY IN 'temptblProject' */
IF UPDATE(Project)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM temptblProject, inserted WHERE (temptblProject.Project = inserted.Project))
            BEGIN
                RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''temptblProject''.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/* * PREVENT UPDATES IF DEPENDENT RECORDS IN 'temptblAdditionalCosts' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, temptblAdditionalCosts WHERE (deleted.Project = temptblAdditionalCosts.Project AND deleted.YearNo = temptblAdditionalCosts.Year)) > 0
            BEGIN
                RAISERROR ( 'The record can''t be deleted or changed. Since related records exist in table ''temptblAdditionalCosts'', referential integrity rules would be violated.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/* * PREVENT UPDATES IF DEPENDENT RECORDS IN 'temptblAnimalReq' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, temptblAnimalReq WHERE (deleted.Project = temptblAnimalReq.Project AND deleted.YearNo = temptblAnimalReq.Year)) > 0
            BEGIN
                RAISERROR ( 'The record can''t be deleted or changed. Since related records exist in table ''temptblAnimalReq'', referential integrity rules would be violated.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/* * PREVENT UPDATES IF DEPENDENT RECORDS IN 'temptblStaffRequ' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, temptblStaffRequ WHERE (deleted.Project = temptblStaffRequ.Project AND deleted.YearNo = temptblStaffRequ.Year)) > 0
            BEGIN
                RAISERROR ( 'The record can''t be deleted or changed. Since related records exist in table ''temptblStaffRequ'', referential integrity rules would be violated.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/* * PREVENT UPDATES IF DEPENDENT RECORDS IN 'temptblTestReq' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, temptblTestReq WHERE (deleted.Project = temptblTestReq.Project AND deleted.YearNo = temptblTestReq.Year)) > 0
            BEGIN
                RAISERROR ( 'The record can''t be deleted or changed. Since related records exist in table ''temptblTestReq'', referential integrity rules would be violated.',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
