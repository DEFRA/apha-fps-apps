USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpSubAccount](
    [SubAccountCode] [varchar](50) NOT NULL,
    [SubAccount] [varchar](50) NULL
,    CONSTRAINT [PK_tlkpSubAccount] PRIMARY KEY CLUSTERED
    (
        SubAccountCode
    )
) ON [PRIMARY]
GO
