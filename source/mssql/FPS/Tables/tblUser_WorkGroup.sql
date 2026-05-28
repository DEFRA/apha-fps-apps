USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_WorkGroup](
    [WorkGroup] [varchar](50) NOT NULL,
    [User_ID] [int] NOT NULL
,    CONSTRAINT [PK___7__10] PRIMARY KEY CLUSTERED
    (
        WorkGroup, User_ID
    )
) ON [PRIMARY]
GO
