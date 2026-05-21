USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tlkpTestReqmt](
    [Year] [smallint] NOT NULL,
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [UnitPrice] [money] NULL,
    [NoRequired] [float] NULL,
    [ProjectBuyerCode] [varchar](50) NULL,
    [TestBuyerCode] [varchar](50) NULL,
    [Source] [char](5) NULL
,    CONSTRAINT [PK_MY_tlkpTestReqmt] PRIMARY KEY CLUSTERED
    (
        Year, TestCode, Buyer
    )
) ON [PRIMARY]
GO
