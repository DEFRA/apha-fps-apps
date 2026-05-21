USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tlkpProject_All](
    [Year] [smallint] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [Program] [varchar](10) NULL,
    [Customer] [varchar](50) NULL,
    [Manager] [varchar](50) NULL,
    [TransferIncome] [money] NULL,
    [CustIncome] [money] NULL,
    [WIP_EOY] [money] NULL,
    [WIP_Limit] [money] NULL,
    [WIP_Current] [money] NULL,
    [ProjectStatus] [varchar](50) NULL,
    [DateCreated] [datetime] NULL,
    [FECcost] [money] NULL,
    [Profit] [money] NULL,
    [Budget_CVL] [money] NULL,
    [CaseworkSub] [decimal](5, 4) NULL,
    [PVSIncome] [money] NULL,
    [PlanCaseworkDebit] [money] NULL,
    [Source] [char](5) NULL,
    [Disease] [varchar](50) NULL,
    [Contract] [varchar](10) NULL,
    [Finished] [smallint] NULL,
    [Comments] [text] NULL,
    [CarryOver] [money] NULL,
    [IsDefraProject] [smallint] NULL,
    [CostCentre] [float] NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [ProjectGroup] [varchar](50) NULL,
    [IncomeAccountCode] [varchar](50) NULL
,    CONSTRAINT [PK_MY_tlkpProject_All] PRIMARY KEY CLUSTERED
    (
        Year, ParentProject
    )
) ON [PRIMARY]
GO
