USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAccessUsers_Levels](
    [SystemID] [int] NOT NULL,
    [NTLogin] [varchar](50) NOT NULL,
    [AccessLevelID] [int] NOT NULL
,    CONSTRAINT [PK_tblAccessUsers_Levels] PRIMARY KEY NONCLUSTERED
    (
        SystemID, NTLogin, AccessLevelID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAccessUsers_Levels] WITH CHECK ADD CONSTRAINT [FK_tblAccessUsers_Levels_tblAccessLevels] FOREIGN KEY(SystemID, AccessLevelID)
REFERENCES [dbo].[tblAccessLevels] (SystemID, AccesslevelID)
GO
ALTER TABLE [dbo].[tblAccessUsers_Levels] CHECK CONSTRAINT [FK_tblAccessUsers_Levels_tblAccessLevels]
GO
ALTER TABLE [dbo].[tblAccessUsers_Levels] WITH CHECK ADD CONSTRAINT [FK_tblAccessUsers_Levels_tblAccessUsers] FOREIGN KEY(SystemID, NTLogin)
REFERENCES [dbo].[tblAccessUsers] (SystemID, NTLogin)
GO
ALTER TABLE [dbo].[tblAccessUsers_Levels] CHECK CONSTRAINT [FK_tblAccessUsers_Levels_tblAccessUsers]
GO
