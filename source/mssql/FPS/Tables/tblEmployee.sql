USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEmployee](
    [SPNumber] [varchar](10) NOT NULL,
    [FirstName] [varchar](20) NULL,
    [LastName] [varchar](20) NULL,
    [Title] [varchar](4) NULL
,    CONSTRAINT [PK___5__10] PRIMARY KEY CLUSTERED
    (
        SPNumber
    )
) ON [PRIMARY]
GO
