USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.TimeCodeValid_ITrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.TimeCodeValid_ITrig    Script Date: 1/11/99 3:48:25 PM ******/
/****** Object:  Trigger dbo.TimeCodeValid_ITrig    Script Date: 10/12/98 2:03:13 PM ******/
/***** CUSTOM DRI *****/
CREATE trigger[dbo].[TimeCodeValid_ITrig] ON [dbo].[TimeCodeValid] FOR INSERT AS
/* Prevent insert if FK combinations are not satisfied.*/
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
 * PREVENT INSERTS IF NO MATCHING KEY IN 'tlkpJobCode'
 */
IF (Select JobCode from inserted) <> ''
Begin
	IF (SELECT COUNT(*) FROM inserted) !=
	   (SELECT COUNT(*) FROM tlkpJobCode, inserted WHERE (tlkpJobCode.JobCode = inserted.JobCode))
		BEGIN
			RAISERROR ( 'Not a valid jobcode.',16,1)
			ROLLBACK TRANSACTION
		END
end
/*
 * PREVENT INSERTS IF NO MATCHING KEYS IN 'tlkpTestCapability'
 */
IF (Select TestCode from inserted) <> '' AND (Select Portfolio from inserted) <> ''
Begin
IF (SELECT COUNT(*) FROM tlkpTestCapability, inserted 
	WHERE (tlkpTestCapability.TestCode = inserted.TestCode AND tlkpTestCapability.PlanPortfolio = inserted.Portfolio)) = 0
    BEGIN
        RAISERROR ( 'Cannot update, this testcode is not in this portfolio.',16,1)
        ROLLBACK TRANSACTION
    END
end
IF (Select ParentProject from inserted) <> ''
Begin
	IF (SELECT COUNT(*) FROM inserted) !=
	   (SELECT COUNT(*) FROM tlkpProject, inserted WHERE (tlkpProject.ParentProject = inserted.ParentProject))
		BEGIN
			RAISERROR ( 'Not a valid project',16,1)
			ROLLBACK TRANSACTION
		END
end


GO
