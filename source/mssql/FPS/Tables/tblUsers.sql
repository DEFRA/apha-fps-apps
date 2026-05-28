USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUsers](
    [User_ID] [int] IDENTITY(1,1) NOT NULL,
    [UserName] [varchar](50) NULL,
    [AgencyID] [int] NULL,
    [frmWarning] [bit] NOT NULL CONSTRAINT [DF__tblUsers__frmWar__1273C1CD] DEFAULT (0),
    [Comments] [varchar](255) NULL,
    [DT2UserName] [varchar](50) NULL
,    CONSTRAINT [PK__tblUsers__1367E606] PRIMARY KEY CLUSTERED
    (
        User_ID
    )
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [UserName] ON [dbo].[tblUsers]
(
    UserName
)
GO
