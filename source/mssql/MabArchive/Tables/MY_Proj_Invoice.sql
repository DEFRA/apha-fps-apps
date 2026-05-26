USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_Proj_Invoice](
    [Year] [smallint] NOT NULL,
    [ProjectParent] [varchar](20) NOT NULL,
    [Month] [int] NULL,
    [Amount] [money] NULL,
    [CostOfWork] [money] NULL,
    [WIP] [money] NULL,
    [ProfitLoss] [money] NULL,
    [Detail] [varchar](100) NULL,
    [InvoiceCounter] [int] NOT NULL,
    [Type] [varchar](10) NULL
,    CONSTRAINT [PK_MY_Proj_Invoice] PRIMARY KEY CLUSTERED
    (
        Year, ProjectParent, InvoiceCounter
    )
) ON [PRIMARY]
GO
