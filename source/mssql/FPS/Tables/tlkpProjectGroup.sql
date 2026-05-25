USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpProjectGroup](
    [ProjectGroup] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tlkpProjectGroup] PRIMARY KEY CLUSTERED
    (
        ProjectGroup
    )
) ON [PRIMARY]
GO
