USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/***** CUSTOM DRI *****/
CREATE trigger[dbo].[TimeCodeValid_UTrig] ON [dbo].[TimeCodeValid] FOR UPDATE AS
IF (SELECT Count(*) FROM inserted where TestCode Is Null and Portfolio Is Null and JobCode Is Null) != 0 
Begin
	RAISERROR ( 'Must fill in Testcode and Portfolio, or Jobcode',16,1)
        ROLLBACK TRANSACTION
End
IF (SELECT COUNT(*) FROM inserted where Portfolio Is Null and TestCode Is Not Null) != 0 
Begin
	RAISERROR ( 'Must fill in Testcode and Portfolio, or Jobcode',16,1)
        ROLLBACK TRANSACTION
End
IF (SELECT COUNT(*) FROM inserted where Portfolio Is Not Null and TestCode Is Null) != 0  
Begin
	RAISERROR ( 'Must fill in Testcode and Portfolio, or Jobcode',16,1)
        ROLLBACK TRANSACTION
End
/*
 * PREVENT UPDATES IF NO MATCHING KEY IN 'tlkpJobCode'
 */
IF (Select JobCode from inserted) Is Not Null
Begin
IF UPDATE(JobCode)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tlkpJobCode, inserted WHERE (tlkpJobCode.JobCode = inserted.JobCode))
            BEGIN
        RAISERROR ( 'Not a valid jobcode.',16,1)
                ROLLBACK TRANSACTION
            END
    END
end
/*
 * PREVENT UPDATES IF DEPENDENT RECORDS IN 'MonthlyTime'
 */
IF UPDATE(WorkGroup) OR UPDATE(TimeCode) OR UPDATE(ParentProject)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, MonthlyTime WHERE (deleted.WorkGroup = MonthlyTime.WorkGroup AND deleted.TimeCode = MonthlyTime.TimeCode AND deleted.ParentProject = MonthlyTime.ParentProject)) > 0
            BEGIN
        RAISERROR ( 'Cannot update, existing data in MonthlyTime.',16,1)
                ROLLBACK TRANSACTION
            END
    END
/*
 * PREVENT UPDATES IF NO MATCHING KEY IN 'tlkpTestCapability'
 */
IF (Select TestCode from inserted) Is Not Null or (Select Portfolio from inserted) Is Not Null
Begin
IF UPDATE(TestCode) OR UPDATE(Portfolio)
    BEGIN
        IF (SELECT COUNT(*) FROM tlkpTestCapability, inserted 
		WHERE (tlkpTestCapability.TestCode = inserted.TestCode AND tlkpTestCapability.PlanPortfolio = inserted.Portfolio)) = 0
            BEGIN
        	RAISERROR ( 'Cannot update, this testcode is not in this portfolio.',16,1)
                ROLLBACK TRANSACTION
            END
	
    END
End
/*
 * PREVENT UPDATES IF NO MATCHING KEY IN 'tlkpProject'
 */
IF UPDATE(ParentProject)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tlkpProject, inserted WHERE (tlkpProject.ParentProject = inserted.ParentProject))
            BEGIN
        RAISERROR ( 'Not a valid project',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
