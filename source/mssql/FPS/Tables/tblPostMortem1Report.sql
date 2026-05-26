USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPostMortem1Report](
    [TestCode] [varchar](20) NOT NULL,
    [ItemDescription] [char](18) NULL,
    [TotVol] [int] NULL,
    [LTUnitCharge] [money] NULL,
    [SDUnitCharge] [money] NULL,
    [LTfee] [money] NULL,
    [SDfee] [money] NULL,
    [Total Fee] [money] NULL,
    [Fee Charged] [money] NULL,
    [Profit/Loss] [money] NULL,
    [WorkGroup] [varchar](50) NULL
) ON [PRIMARY]
GO
