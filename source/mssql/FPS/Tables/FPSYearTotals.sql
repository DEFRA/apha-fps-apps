USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FPSYearTotals](
    [ParentProject] [varchar](20) NOT NULL,
    [Program] [varchar](10) NOT NULL,
    [TotalAdditionalCosts] [money] NULL,
    [TotalAnimalCosts] [float] NULL,
    [TotalStaffCosts] [float] NULL,
    [TotalTestCosts] [float] NULL,
    [TotalCosts] [float] NULL,
    [CustIncome] [money] NOT NULL,
    [TransferIncome] [money] NOT NULL,
    [TotalIncome] [money] NOT NULL,
    [Budget_CVL] [money] NULL,
    [RequiredProfit] [money] NULL,
    [Manager] [varchar](50) NULL,
    [Customer] [varchar](50) NULL,
    [ProjectStatus] [varchar](50) NULL,
    [PVSIncome] [money] NULL,
    [PlanCaseworkDebit] [money] NULL,
    [TotalPayCosts] [float] NULL
,    CONSTRAINT [PK_FPSYearTotals] PRIMARY KEY CLUSTERED
    (
        ParentProject
    )
) ON [PRIMARY]
GO
