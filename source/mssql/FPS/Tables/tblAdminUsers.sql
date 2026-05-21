USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAdminUsers](
    [mNumber] [varchar](50) NOT NULL,
    [Name] [varchar](50) NOT NULL,
    [SeeDeptIncome] [bit] NOT NULL CONSTRAINT [DF_tblAdminUsers_SeeDeptIncome] DEFAULT ((0)),
    [SeeDBWindow] [bit] NOT NULL CONSTRAINT [DF_tblAdminUsers_SeeDBWindow] DEFAULT ((0)),
    [DT2Number] [varchar](50) NULL
,    CONSTRAINT [PK_tblAdminUsers] PRIMARY KEY CLUSTERED
    (
        mNumber
    )
) ON [PRIMARY]
GO
