USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpProject](
    [ParentProject] [varchar](20) NOT NULL,
    [ProjectTitle] [varchar](200) NOT NULL,
    [Program] [varchar](10) NOT NULL,
    [Customer] [varchar](50) NOT NULL,
    [Manager] [varchar](50) NULL,
    [TransferIncome] [money] NOT NULL,
    [CustIncome] [money] NOT NULL,
    [WIP_EOY] [money] NULL CONSTRAINT [DF_tlkpProject_WIP_EOY] DEFAULT ((0)),
    [WIP_Limit] [money] NULL,
    [WIP_Current] [money] NULL,
    [ProjectStatus] [varchar](50) NOT NULL,
    [CostBookNo] [varchar](50) NULL,
    [DateCreated] [datetime] NULL CONSTRAINT [DF__tlkpProje__DateC__68B2922B] DEFAULT (getdate()),
    [FECcost] [money] NULL CONSTRAINT [DF__tlkpProje__FECco__69A6B664] DEFAULT ((0)),
    [Profit] [money] NULL CONSTRAINT [DF__tlkpProje__Profi__6A9ADA9D] DEFAULT ((0)),
    [Budget_CVL] [money] NULL CONSTRAINT [DF__tlkpProje__Contr__6B8EFED6] DEFAULT ((0)),
    [DateCosted] [datetime] NULL,
    [Disease] [varchar](50) NOT NULL,
    [Contract] [varchar](10) NOT NULL,
    [ProjectParent] [varchar](50) NULL,
    [ShortTitle] [varchar](30) NULL,
    [CaseworkSub] [decimal](5, 4) NULL,
    [PVSIncome] [money] NULL,
    [PlanCaseworkDebit] [money] NULL,
    [Finished] [smallint] NULL CONSTRAINT [DF_tlkpProject_Finished] DEFAULT ((0)),
    [OwningRC] [varchar](50) NULL,
    [Comments] [text] NULL,
    [CarryOver] [money] NULL,
    [CarryOverSeed] [money] NULL,
    [IsDefraProject] [smallint] NOT NULL,
    [CostCentre] [float] NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [ProjectGroup] [varchar](50) NULL,
    [IncomeAccountCode] [varchar](50) NOT NULL
,    CONSTRAINT [PK__tlkpProject__6C83230F] PRIMARY KEY CLUSTERED
    (
        ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_1__10] FOREIGN KEY(ProjectStatus)
REFERENCES [dbo].[tblStatus] (Status)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_1__10]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_1__16] FOREIGN KEY(Customer)
REFERENCES [dbo].[tlkpCustomer] (Customer)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_1__16]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_2__10] FOREIGN KEY(Disease)
REFERENCES [dbo].[tblDisease] (Disease)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_2__10]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_3__10] FOREIGN KEY(Program)
REFERENCES [dbo].[tlkpProgram] (ProgramNo)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_3__10]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_4__10] FOREIGN KEY(Contract)
REFERENCES [dbo].[tblContract] (ContractNo)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_4__10]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_tlkpAccountCode] FOREIGN KEY(IncomeAccountCode)
REFERENCES [dbo].[tlkpAccountCode] (Code)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_tlkpAccountCode]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_tlkpProjectGroup] FOREIGN KEY(ProjectGroup)
REFERENCES [dbo].[tlkpProjectGroup] (ProjectGroup)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_tlkpProjectGroup]
GO
ALTER TABLE [dbo].[tlkpProject] WITH CHECK ADD CONSTRAINT [FK_tlkpProject_tlkpSubAccount] FOREIGN KEY(SubAccountCode)
REFERENCES [dbo].[tlkpSubAccount] (SubAccountCode)
GO
ALTER TABLE [dbo].[tlkpProject] CHECK CONSTRAINT [FK_tlkpProject_tlkpSubAccount]
GO
CREATE NONCLUSTERED INDEX [ProjectStatus] ON [dbo].[tlkpProject]
(
    ProjectStatus
)
GO
