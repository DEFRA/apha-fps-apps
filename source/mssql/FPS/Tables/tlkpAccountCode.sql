USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpAccountCode](
    [Code] [varchar](50) NOT NULL,
    [Description] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tlkpAccountCode] PRIMARY KEY CLUSTERED
    (
        Code
    )
) ON [PRIMARY]
GO
