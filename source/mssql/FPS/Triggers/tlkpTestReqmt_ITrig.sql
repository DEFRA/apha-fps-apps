USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE trigger[dbo].[tlkpTestReqmt_ITrig] ON [dbo].[tlkpTestReqmt] FOR INSERT AS
					/* CUSTOM DRI */
IF (SELECT Count(*) FROM inserted where ProjectBuyerCode Is Null and TestBuyerCode Is Null) != 0 
Begin
	RAISERROR ( 'Must fill in Project Buyer or Test Buyer',16,1)
        ROLLBACK TRANSACTION
End
/*
 * PREVENT INSERTS IF NO MATCHING KEY IN 'tlkpProject'
 */
IF (Select ProjectBuyerCode from inserted) Is Not Null
IF (SELECT COUNT(*) FROM inserted) !=
   (SELECT COUNT(*) FROM tlkpProject, inserted WHERE (tlkpProject.ParentProject = inserted.ProjectBuyerCode))
    BEGIN
        RAISERROR ( 'Not a valid project.',16,1)
        ROLLBACK TRANSACTION
    END
/*
 * PREVENT INSERTS IF NO MATCHING KEY IN 'tlkpTestCapability'
 */
IF (Select TestBuyerCode from inserted) Is Not Null
IF (SELECT COUNT(*) FROM inserted) !=
   (SELECT COUNT(*) FROM tlkptestcapability, inserted WHERE (tlkptestcapability.testcode + tlkptestcapability.workgroup = inserted.testBuyerCode))
    BEGIN
        RAISERROR ( 'This workgroup is not setup to do this test.',16,1)
        ROLLBACK TRANSACTION
    END


GO
