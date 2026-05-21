USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestRCCost](
    [TestCode] [varchar](20) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [Price] [money] NOT NULL CONSTRAINT [DF_tblTestRCCost_Price] DEFAULT (0)
,    CONSTRAINT [PK_tblTestRCCost] PRIMARY KEY NONCLUSTERED
    (
        TestCode, ProfitCentre
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblTestRCCost] WITH CHECK ADD CONSTRAINT [FK_tblTestRCCost_tblkpProfitCentre] FOREIGN KEY(ProfitCentre)
REFERENCES [dbo].[tblkpProfitCentre] (ProfitCentre)
GO
ALTER TABLE [dbo].[tblTestRCCost] CHECK CONSTRAINT [FK_tblTestRCCost_tblkpProfitCentre]
GO
ALTER TABLE [dbo].[tblTestRCCost] WITH CHECK ADD CONSTRAINT [FK_tblTestRCCost_TestOrProduct] FOREIGN KEY(TestCode)
REFERENCES [dbo].[TestOrProduct] (ItemCode)
GO
ALTER TABLE [dbo].[tblTestRCCost] CHECK CONSTRAINT [FK_tblTestRCCost_TestOrProduct]
GO
