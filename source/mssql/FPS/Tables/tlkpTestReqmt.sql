USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpTestReqmt](
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [UnitPrice] [money] NULL,
    [NoRequired] [float] NULL,
    [ProjectBuyerCode] [varchar](50) NULL,
    [TestBuyerCode] [varchar](50) NULL,
    [DateCreated] [datetime] NULL CONSTRAINT [DF_tlkpTestRe_DateCreated1__15] DEFAULT (getdate()),
    [Active] [tinyint] NULL CONSTRAINT [DF_tlkpTestRe_Active_1__16] DEFAULT (1)
,    CONSTRAINT [aaaaatlkpTestReqmt_PK] PRIMARY KEY NONCLUSTERED
    (
        TestCode, Buyer
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tlkpTestReqmt] WITH CHECK ADD CONSTRAINT [FK_tlkpTestReqmt_1__11] FOREIGN KEY(TestCode)
REFERENCES [dbo].[TestOrProduct] (ItemCode)
GO
ALTER TABLE [dbo].[tlkpTestReqmt] CHECK CONSTRAINT [FK_tlkpTestReqmt_1__11]
GO
CREATE NONCLUSTERED INDEX [Reference10] ON [dbo].[tlkpTestReqmt]
(
    TestBuyerCode
)
GO
CREATE NONCLUSTERED INDEX [Reference19] ON [dbo].[tlkpTestReqmt]
(
    ProjectBuyerCode
)
GO
CREATE NONCLUSTERED INDEX [Reference31] ON [dbo].[tlkpTestReqmt]
(
    TestCode
)
GO
