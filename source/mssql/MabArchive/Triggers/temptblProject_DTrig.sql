USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblProject_DTrig] ON [dbo].[temptblProject] FOR DELETE AS
SET NOCOUNT ON
/* * CASCADE DELETES TO 'temptblProjectYear' */
DELETE temptblProjectYear FROM deleted, temptblProjectYear WHERE deleted.Project = temptblProjectYear.Project


GO
