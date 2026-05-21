USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProjectManager](
    [ProjectManager] [varchar](50) NOT NULL,
    [Email] [varchar](255) NULL,
    [MNumber] [varchar](10) NULL,
    [Disable] [bit] NOT NULL CONSTRAINT [DF_tblProjectManager_Disable] DEFAULT ((0))
,    CONSTRAINT [PK_tblProjectManager] PRIMARY KEY NONCLUSTERED
    (
        ProjectManager
    )
) ON [PRIMARY]
GO
