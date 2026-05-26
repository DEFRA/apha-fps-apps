USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthFinal](
    [Project] [varchar](20) NOT NULL,
    [MonthNo] [float] NOT NULL,
    [PeriodName] [varchar](50) NULL,
    [CumFlag] [float] NULL,
    [CostProfile] [money] NULL,
    [Subcontracts] [money] NULL,
    [Animals] [money] NULL,
    [NonAnimals] [money] NULL,
    [TimeCosts] [money] NULL,
    [TransferCosts] [money] NULL,
    [TotalCost] [money] NULL,
    [Invoices] [money] NULL,
    [COIW] [money] NULL,
    [PortSales] [money] NULL,
    [CumCost] [money] NULL,
    [CumProfile] [money] NULL,
    [SumOfCostProfile] [money] NULL,
    [CumInvoices] [money] NULL,
    [CumCOIW] [money] NULL,
    [CumPortSales] [money] NULL,
    [MstoneDue] [int] NULL,
    [Due__Done] [float] NULL,
    [OnTime] [float] NULL,
    [SumOfMstoneDue] [float] NULL,
    [SumOfDue__Done] [float] NULL,
    [SumOfOnTime] [float] NULL,
    [CWDebit] [money] NULL,
    [CWCredit] [money] NULL,
    [CumCWDebit] [money] NULL,
    [CumCWCredit] [money] NULL,
    [TotalHours] [float] NULL,
    [CumTotalHours] [float] NULL,
    [CumSubContracts] [float] NULL,
    [x] [int] NULL,
    [CumTestCosts] [float] NULL,
    [PayCosts] [float] NULL,
    [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonthFinal_PK] PRIMARY KEY NONCLUSTERED
    (
        Project, MonthNo
    )
) ON [PRIMARY]
GO
