USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpTestCapability](
    [TestCode] [varchar](20) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [PlanPortfolio] [varchar](20) NOT NULL,
    [UnitCost] [money] NULL CONSTRAINT [DF__tlkpTestC__UnitC__4C6B5938] DEFAULT (0),
    [PredOutturn] [float] NULL CONSTRAINT [DF__tlkpTestC__PredO__4D5F7D71] DEFAULT (0),
    [SOP] [varchar](50) NULL,
    [SMScode] [varchar](50) NULL
,    CONSTRAINT [PK__tlkpTestCapabili__4E53A1AA] PRIMARY KEY CLUSTERED
    (
        TestCode, WorkGroup
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tlkpTestCapability] WITH CHECK ADD CONSTRAINT [FK_tlkpTestCapability_1__15] FOREIGN KEY(WorkGroup)
REFERENCES [dbo].[WorkGroup] (WorkGroup)
GO
ALTER TABLE [dbo].[tlkpTestCapability] CHECK CONSTRAINT [FK_tlkpTestCapability_1__15]
GO
ALTER TABLE [dbo].[tlkpTestCapability] WITH CHECK ADD CONSTRAINT [FK_tlkpTestCapability_1__18] FOREIGN KEY(PlanPortfolio)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[tlkpTestCapability] CHECK CONSTRAINT [FK_tlkpTestCapability_1__18]
GO
ALTER TABLE [dbo].[tlkpTestCapability] WITH CHECK ADD CONSTRAINT [FK_tlkpTestCapability_2__18] FOREIGN KEY(TestCode)
REFERENCES [dbo].[TestOrProduct] (ItemCode)
GO
ALTER TABLE [dbo].[tlkpTestCapability] CHECK CONSTRAINT [FK_tlkpTestCapability_2__18]
GO
CREATE NONCLUSTERED INDEX [tlkpTestCapability_PlanPortfol] ON [dbo].[tlkpTestCapability]
(
    PlanPortfolio
)
GO
