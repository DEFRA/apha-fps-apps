USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Project_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [ProjectTitle] [varchar](200) NOT NULL,
    [Program] [varchar](10) NOT NULL,
    [Customer] [varchar](50) NOT NULL,
    [Manager] [varchar](50) NULL,
    [TransferIncome] [money] NOT NULL,
    [CustIncome] [money] NOT NULL,
    [WIP_EOY] [money] NULL,
    [WIP_Limit] [money] NULL,
    [WIP_Current] [money] NULL,
    [ProjectStatus] [varchar](50) NOT NULL,
    [CostBookNo] [varchar](50) NULL,
    [DateCreated] [datetime] NULL,
    [FECcost] [money] NULL,
    [Profit] [money] NULL,
    [Budget_CVL] [money] NULL,
    [DateCosted] [datetime] NULL,
    [Disease] [varchar](50) NOT NULL,
    [Contract] [varchar](10) NOT NULL,
    [ProjectParent] [varchar](50) NULL,
    [ShortTitle] [varchar](30) NULL,
    [CaseworkSub] [decimal](5, 4) NULL,
    [PVSIncome] [money] NULL,
    [PlanCaseworkDebit] [money] NULL,
    [Finished] [smallint] NULL,
    [OwningRC] [varchar](50) NULL,
    [Comments] [text] NULL,
    [CarryOver] [money] NULL,
    [CarryOverSeed] [money] NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL,
    [JobCode] AS ([ParentProject]),
    [IsDefraProject] [smallint] NULL,
    [CostCentre] [float] NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [ProjectGroup] [varchar](50) NULL,
    [IncomeAccountCode] [varchar](50) NULL
,    CONSTRAINT [PK_Project_Log] PRIMARY KEY NONCLUSTERED
    (
        SequenceNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Ind_Dt] ON [dbo].[Project_Log]
(
    Date_Time
)
GO
CREATE NONCLUSTERED INDEX [Ind_JC] ON [dbo].[Project_Log]
(
    JobCode
)
GO
