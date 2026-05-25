USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger[dbo].[tlkpJobCode_UTrig] ON [dbo].[tlkpJobCode] FOR UPDATE AS

/*
 * PREVENT UPDATES IF DEPENDENT RECORDS IN 'TimeCodeValid'  */
IF UPDATE(JobCode)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, TimeCodeValid WHERE (deleted.JobCode = TimeCodeValid.JobCode)) > 0
            BEGIN
        RAISERROR('This Jobcode has related records in TimeCodeValid, delete aborted.', 16, 1)
                ROLLBACK TRANSACTION
            END 
    END


GO
