USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth3](
    [EndPeriod] [float] NOT NULL,
    [PeriodName] [varchar](50) NULL,
    [Project] [varchar](20) NOT NULL,
    [CumCost] [money] NULL,
    [CumInvoices] [money] NULL,
    [CumCOIW] [money] NULL,
    [CumPortSales] [float] NULL,
    [CumProfile] [money] NULL,
    [SumOfCostProfile] [money] NULL,
    [SumOfMstoneDue] [float] NULL,
    [SumOfDue__Done] [float] NULL,
    [SumOfOnTime] [float] NULL,
    [CumCWDebit] [money] NULL,
    [CumCWCredit] [money] NULL,
    [CumTotalHours] [float] NULL,
    [CumSubContracts] [float] NULL,
    [CumTestCosts] [float] NULL,
    [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth3_PK] PRIMARY KEY NONCLUSTERED
    (
        EndPeriod, Project
    )
) ON [PRIMARY]
GO
