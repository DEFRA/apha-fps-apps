USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth2](
    [Project] [varchar](20) NOT NULL,
    [MonthNo] [float] NOT NULL,
    [CostProfile] [money] NULL,
    [Subcontracts] [money] NULL,
    [Animals] [money] NULL,
    [NonAnimal] [money] NULL,
    [TimeCosts] [float] NULL,
    [TransferCosts] [float] NULL,
    [TotalCost] [money] NULL,
    [Invoices] [money] NULL,
    [COIW] [money] NULL,
    [SumOfCostProfile] [money] NULL,
    [PortSales] [float] NULL,
    [MstoneDue] [int] NULL,
    [Due__Done] [float] NULL,
    [OnTime] [float] NULL,
    [TotalHours] [float] NULL,
    [PayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth2_PK] PRIMARY KEY NONCLUSTERED
    (
        Project, MonthNo
    )
) ON [PRIMARY]
GO
