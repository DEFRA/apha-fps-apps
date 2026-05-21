USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl Settings](
    [ID] [nvarchar](50) NOT NULL,
    [Setting] [nvarchar](255) NULL,
    [Notes] [nvarchar](255) NULL,
    [TestSetting] [nvarchar](255) NULL,
    [UserUpdateable] [bit] NULL CONSTRAINT [DF__Temporary__UserU__0CBAE877] DEFAULT (0)
,    CONSTRAINT [aaaaatbl Settings_PK] PRIMARY KEY NONCLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [SettingID] ON [dbo].[tbl Settings]
(
    ID
)
GO
