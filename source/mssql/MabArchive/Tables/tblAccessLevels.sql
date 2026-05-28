USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAccessLevels](
    [SystemID] [int] NOT NULL,
    [AccesslevelID] [int] NOT NULL,
    [AccessLevel] [varchar](50) NULL
,    CONSTRAINT [PK_tblAccessLevels] PRIMARY KEY NONCLUSTERED
    (
        SystemID, AccesslevelID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAccessLevels] WITH CHECK ADD CONSTRAINT [FK_tblAccessLevels_tblAccessSystems] FOREIGN KEY(SystemID)
REFERENCES [dbo].[tblAccessSystems] (SystemID)
GO
ALTER TABLE [dbo].[tblAccessLevels] CHECK CONSTRAINT [FK_tblAccessLevels_tblAccessSystems]
GO
