USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSettings](
    [ID] [varchar](50) NOT NULL,
    [Setting] [varchar](255) NULL,
    [Notes] [varchar](255) NULL,
    [TestSetting] [varchar](255) NULL
,    CONSTRAINT [PK_tblSettings] PRIMARY KEY NONCLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
