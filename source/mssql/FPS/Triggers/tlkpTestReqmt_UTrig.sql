USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tlkpTestReqmt_UTrig    Script Date: 3/4/00 1:48:24 PM ******/
CREATE trigger[dbo].[tlkpTestReqmt_UTrig] on [dbo].[tlkpTestReqmt]
  for UPDATE
  as
IF (SELECT Count(*) FROM inserted where ProjectBuyerCode Is Null and TestBuyerCode Is Null) != 0 
Begin
	RAISERROR ( 'Cannot update, you must fill in project buyer or test buyer.',16,1)
        ROLLBACK TRANSACTION
End

/*
 * PREVENT UPDATES IF NO MATCHING KEY IN 'tlkpTestCapability'
 */
--IF (Select TestBuyerCode from inserted) Is Not Null
IF UPDATE(TestBuyerCode)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tlkpTestCapability, inserted WHERE (tlkpTestCapability.TestCode + tlkpTestCapability.workgroup= inserted.TestBuyerCode))
            BEGIN
        RAISERROR ( 'Cannot update, test buyers workgroup is not setup to do this test.',16,1)
                ROLLBACK TRANSACTION
            END
    END

/*
 * PREVENT UPDATES IF DEPENDENT RECORDS IN 'MonthlyOutput'
 */
IF UPDATE(TestCode) OR UPDATE(Buyer)
    BEGIN
        IF (SELECT COUNT(*) FROM deleted, MonthlyOutput WHERE (deleted.TestCode = MonthlyOutput.TestCode AND deleted.Buyer = MonthlyOutput.Buyer)) > 0
            BEGIN
        RAISERROR ( 'Cannot update, existing data in Monthly Output.',16,1)
       ROLLBACK TRANSACTION
            END
    END

/*
 * PREVENT UPDATES IF NO MATCHING KEY IN 'tlkpProject'
 */
--IF (Select ProjectBuyerCode from inserted) Is Not Null
IF UPDATE(ProjectBuyerCode)
    BEGIN
        IF (SELECT COUNT(*) FROM inserted) !=
           (SELECT COUNT(*) FROM tlkpProject, inserted WHERE (tlkpProject.ParentProject = inserted.ProjectBuyerCode))
            BEGIN
        RAISERROR ( 'Cannot update, project does not exist.',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
