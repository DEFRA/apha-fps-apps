USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblProject_DTrig] ON [dbo].[tblProject] FOR DELETE AS
SET NOCOUNT ON
/* * CASCADE DELETES TO 'tblProjectYear' */
DELETE tblProjectYear FROM deleted, tblProjectYear WHERE deleted.Project = tblProjectYear.Project


GO
