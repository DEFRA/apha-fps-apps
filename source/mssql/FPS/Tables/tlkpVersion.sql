USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpVersion](
    [Version] [int] NULL,
    [x] [binary](1) NULL,
    [IsLive] [int] NULL
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [Version_PK] ON [dbo].[tlkpVersion]
(
    Version
)
GO
