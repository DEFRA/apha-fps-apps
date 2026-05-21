USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_category](
    [User_ID] [int] NOT NULL,
    [Category] [varchar](20) NOT NULL
,    CONSTRAINT [PK___6__10] PRIMARY KEY CLUSTERED
    (
        User_ID, Category
    )
) ON [PRIMARY]
GO
