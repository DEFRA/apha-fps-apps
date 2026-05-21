USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthlyOutput](
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [Month] [float] NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [Volume] [float] NULL,
    [WGBuyer] [varchar](50) NULL
,    CONSTRAINT [PK_MonthlyOutput] PRIMARY KEY CLUSTERED
    (
        TestCode, Buyer, Month, WorkGroup
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MonthlyOutput] WITH CHECK ADD CONSTRAINT [FK_MonthlyOutput_1__16] FOREIGN KEY(Buyer, TestCode)
REFERENCES [dbo].[tlkpTestReqmt] (Buyer, TestCode)
GO
ALTER TABLE [dbo].[MonthlyOutput] CHECK CONSTRAINT [FK_MonthlyOutput_1__16]
GO
ALTER TABLE [dbo].[MonthlyOutput] WITH CHECK ADD CONSTRAINT [FK_MonthlyOutput_2__11] FOREIGN KEY(TestCode, WorkGroup)
REFERENCES [dbo].[tlkpTestCapability] (TestCode, WorkGroup)
GO
ALTER TABLE [dbo].[MonthlyOutput] CHECK CONSTRAINT [FK_MonthlyOutput_2__11]
GO
CREATE NONCLUSTERED INDEX [Month] ON [dbo].[MonthlyOutput]
(
    Month
)
GO
CREATE NONCLUSTERED INDEX [Reference14] ON [dbo].[MonthlyOutput]
(
    TestCode, Buyer
)
GO
CREATE NONCLUSTERED INDEX [Reference25] ON [dbo].[MonthlyOutput]
(
    TestCode, WorkGroup
)
GO
CREATE NONCLUSTERED INDEX [TestCode] ON [dbo].[MonthlyOutput]
(
    TestCode
)
GO
CREATE NONCLUSTERED INDEX [WorkGroup] ON [dbo].[MonthlyOutput]
(
    WorkGroup
)
GO
CREATE NONCLUSTERED INDEX [WorkGroup_TestCode] ON [dbo].[MonthlyOutput]
(
    WorkGroup, TestCode
)
GO
