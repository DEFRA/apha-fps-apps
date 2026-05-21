USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_Program](
    [User_ID] [int] NOT NULL,
    [ProgramNo] [varchar](10) NOT NULL
,    CONSTRAINT [PK__tblUser_Program__26AFC4A4] PRIMARY KEY CLUSTERED
    (
        User_ID, ProgramNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [XIF84tblUser_Program] ON [dbo].[tblUser_Program]
(
    ProgramNo
)
GO
