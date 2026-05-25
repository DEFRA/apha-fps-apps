USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_TestOwner](
    [User_ID] [int] NOT NULL,
    [Test_Owner] [varchar](2) NOT NULL
,    CONSTRAINT [PK___1__25] PRIMARY KEY CLUSTERED
    (
        User_ID, Test_Owner
    )
) ON [PRIMARY]
GO
