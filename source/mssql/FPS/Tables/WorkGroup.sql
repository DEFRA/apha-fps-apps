USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkGroup](
    [WorkGroup] [varchar](50) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [CostCentre] [float] NULL,
    [Owner] [varchar](50) NULL,
    [Description] [varchar](45) NULL,
    [CentralOverhead] [money] NULL CONSTRAINT [DF__WorkGroup__Centr__245D67DE] DEFAULT (0),
    [SysTimeStamp] [timestamp] NULL,
    [SendEmail] [tinyint] NULL,
    [COS90] [tinyint] NULL,
    [CostCentreOld] [float] NULL,
    [Email_Recipient] [varchar](50) NULL
,    CONSTRAINT [PK__WorkGroup__25518C17] PRIMARY KEY CLUSTERED
    (
        WorkGroup
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WorkGroup] WITH CHECK ADD CONSTRAINT [FK_WorkGroup_1__10] FOREIGN KEY(ProfitCentre)
REFERENCES [dbo].[tblkpProfitCentre] (ProfitCentre)
GO
ALTER TABLE [dbo].[WorkGroup] CHECK CONSTRAINT [FK_WorkGroup_1__10]
GO
ALTER TABLE [dbo].[WorkGroup] WITH CHECK ADD CONSTRAINT [FK_WorkGroup_CostCentre] FOREIGN KEY(CostCentre)
REFERENCES [dbo].[CostCentre] (CostCentre)
GO
ALTER TABLE [dbo].[WorkGroup] CHECK CONSTRAINT [FK_WorkGroup_CostCentre]
GO
CREATE NONCLUSTERED INDEX [ProfitCentre] ON [dbo].[WorkGroup]
(
    ProfitCentre
)
GO
