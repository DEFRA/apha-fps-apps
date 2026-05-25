USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblProjectYear_UTrig] ON [dbo].[tblProjectYear] FOR UPDATE AS
SET NOCOUNT ON
/* * PREVENT UPDATES IF NO MATCHING KEY IN 'tblProject' */
IF UPDATE(Project)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tblProject, inserted WHERE (tblProject.Project = inserted.Project))
            BEGIN
                RAISERROR ( 'The record can''t be added or changed. Referential integrity rules require a related record in table ''tblProject''.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/* * CASCADE UPDATES TO 'tblAdditionalCosts' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
       UPDATE tblAdditionalCosts
       SET tblAdditionalCosts.Project = inserted.Project , tblAdditionalCosts.Year = inserted.YearNo
       FROM tblAdditionalCosts, deleted, inserted
       WHERE deleted.Project = tblAdditionalCosts.Project AND deleted.YearNo = tblAdditionalCosts.Year
    END
/* * CASCADE UPDATES TO 'tblAnimalReq' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
       UPDATE tblAnimalReq
       SET tblAnimalReq.Project = inserted.Project , tblAnimalReq.Year = inserted.YearNo
       FROM tblAnimalReq, deleted, inserted
       WHERE deleted.Project = tblAnimalReq.Project AND deleted.YearNo = tblAnimalReq.Year
    END
/* * CASCADE UPDATES TO 'tblStaffRequ' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
       UPDATE tblStaffRequ
       SET tblStaffRequ.Project = inserted.Project , tblStaffRequ.Year = inserted.YearNo
       FROM tblStaffRequ, deleted, inserted
       WHERE deleted.Project = tblStaffRequ.Project AND deleted.YearNo = tblStaffRequ.Year
    END
/* * CASCADE UPDATES TO 'tblTestRequ' */
IF UPDATE(Project) OR UPDATE(YearNo)
    BEGIN
       UPDATE tblTestRequ
       SET tblTestRequ.Project = inserted.Project , tblTestRequ.Year = inserted.YearNo
       FROM tblTestRequ, deleted, inserted
       WHERE deleted.Project = tblTestRequ.Project AND deleted.YearNo = tblTestRequ.Year
    END


GO
