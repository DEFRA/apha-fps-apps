USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_ProjectGroup](
    [User_ID] [int] NOT NULL,
    [ProjectGroup] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblUser_ProjectGroup] PRIMARY KEY CLUSTERED
    (
        User_ID, ProjectGroup
    )
) ON [PRIMARY]
GO
