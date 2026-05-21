USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblProject_UTrig] ON [dbo].[tblProject] FOR UPDATE AS
SET NOCOUNT ON
/* * CASCADE UPDATES TO 'tblProjectYear' */
IF UPDATE(Project)
    BEGIN
       UPDATE tblProjectYear
       SET tblProjectYear.Project = inserted.Project
       FROM tblProjectYear, deleted, inserted
       WHERE deleted.Project = tblProjectYear.Project
    END


GO
