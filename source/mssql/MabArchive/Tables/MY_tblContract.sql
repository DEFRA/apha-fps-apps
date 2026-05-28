USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblContract](
    [Year] [smallint] NOT NULL,
    [ContractNo] [varchar](10) NOT NULL,
    [Category] [varchar](20) NOT NULL,
    [Manager] [varchar](50) NULL,
    [Customer] [varchar](50) NULL,
    [Title] [varchar](100) NULL,
    [RegisteredDate] [datetime] NULL,
    [StartDate] [datetime] NULL,
    [EndDate] [datetime] NULL,
    [ContractDoc] [image] NULL,
    [Duration] [int] NULL
,    CONSTRAINT [PK_MY_tblContract] PRIMARY KEY CLUSTERED
    (
        Year, ContractNo
    )
) ON [PRIMARY]
GO
