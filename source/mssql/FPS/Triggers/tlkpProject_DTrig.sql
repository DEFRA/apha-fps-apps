USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tlkpProject_DTrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tlkpProject_DTrig    Script Date: 1/11/99 3:48:25 PM ******/
/****** Object:  Trigger dbo.tlkpProject_DTrig    Script Date: 10/19/98 11:52:16 AM ******/
/***** CUSTOM DRI *****/
CREATE trigger[dbo].[tlkpProject_DTrig] ON dbo.tlkpProject FOR DELETE AS
/*
 * CUSTOM DRI - PREVENT DELETES IF DEPENDENT RECORDS IN 'tlkpTestReqmt'
 */
IF (SELECT COUNT(*) FROM deleted, tlkpTestReqmt WHERE (deleted.ParentProject = tlkpTestReqmt.ProjectBuyerCode)) > 0
    BEGIN
        RAISERROR ( 'Cannot delete project, it still has tests planned.',16,1)
        ROLLBACK TRANSACTION
    END

GO
