USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestRequirementRCCost](
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [Price] [money] NOT NULL
,    CONSTRAINT [PK_tblTestRequirementRCCost] PRIMARY KEY CLUSTERED
    (
        TestCode, Buyer, ProfitCentre
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblTestRequirementRCCost] WITH CHECK ADD CONSTRAINT [FK_tblTestRequirementRCCost_tblTestRCCost] FOREIGN KEY(TestCode, ProfitCentre)
REFERENCES [dbo].[tblTestRCCost] (TestCode, ProfitCentre)
GO
ALTER TABLE [dbo].[tblTestRequirementRCCost] CHECK CONSTRAINT [FK_tblTestRequirementRCCost_tblTestRCCost]
GO
ALTER TABLE [dbo].[tblTestRequirementRCCost] WITH CHECK ADD CONSTRAINT [FK_tblTestRequirementRCCost_tlkpTestReqmt] FOREIGN KEY(Buyer, TestCode)
REFERENCES [dbo].[tlkpTestReqmt] (Buyer, TestCode)
GO
ALTER TABLE [dbo].[tblTestRequirementRCCost] CHECK CONSTRAINT [FK_tblTestRequirementRCCost_tlkpTestReqmt]
GO
