USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpManager](
    [Manager] [varchar](50) NOT NULL,
    [Title] [varchar](10) NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL
,    CONSTRAINT [PK___1__18] PRIMARY KEY CLUSTERED
    (
        Manager
    )
) ON [PRIMARY]
GO
