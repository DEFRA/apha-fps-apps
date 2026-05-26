USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tlkpTestReqmt_Dtrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tlkpTestReqmt_Dtrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tlkpTestReqmt_Dtrig] ON [dbo].[tlkpTestReqmt] 
FOR DELETE 
AS    
BEGIN
        IF (SELECT COUNT(*) FROM deleted, MonthlyOutput WHERE (deleted.TestCode = MonthlyOutput.TestCode AND deleted.Buyer = MonthlyOutput.Buyer)) > 0
            BEGIN
        RAISERROR ( 'Cannot delete, existing data in MonthlyOutput.',16,1)
                ROLLBACK TRANSACTION
            END
    END


GO
