USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tlkpTestCapability_DTrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tlkpTestCapability_DTrig    Script Date: 1/11/99 3:48:25 PM ******/
/****** Object:  Trigger dbo.tlkpTestCapability_DTrig    Script Date: 10/19/98 11:53:49 AM ******/
/***** CUSTOM DRI *****/
/****** Object:  Trigger dbo.tlkpTestCapability_DTrig    Script Date: 31/12/97 14:51:25 ******/
CREATE trigger[dbo].[tlkpTestCapability_DTrig] ON [dbo].[tlkpTestCapability] FOR DELETE AS
/*
 * PREVENT DELETES IF DEPENDENT RECORDS IN 'tlkpTestReqmt'
 */
IF (SELECT COUNT(*) FROM deleted, tlkpTestReqmt WHERE (deleted.TestCode + deleted.WorkGroup  = tlkpTestReqmt.TestBuyerCode)) > 0
    BEGIN
        RAISERROR (  'Cannot delete, test requirements are dependant on this.',16,1)
        ROLLBACK TRANSACTION
    END


GO
