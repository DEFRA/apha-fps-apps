USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblReport](
    [ID] [int] NOT NULL,
    [ReportName] [varchar](50) NOT NULL,
    [ReportDescription] [varchar](50) NULL,
    [Filter] [varchar](200) NULL,
    [MailComment] [varchar](250) NULL,
    [MailTitle] [varchar](50) NULL,
    [Emailable] [bit] NOT NULL,
    [SortOrder] [int] NULL,
    [AllowPickProgramme] [bit] NOT NULL,
    [AllowPickProject] [bit] NOT NULL,
    [AllowPickManager] [bit] NOT NULL,
    [AllowPickContract] [bit] NOT NULL,
    [AllowPickCustomer] [bit] NOT NULL,
    [AllowPickMonth] [bit] NOT NULL,
    [AllowPickFYear] [bit] NOT NULL,
    [ReportHelp] [varchar](250) NULL,
    [Type] [char](1) NOT NULL
,    CONSTRAINT [PK_tblReport] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
