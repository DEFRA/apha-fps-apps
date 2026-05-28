USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestReqBaseline](
    [Program] [varchar](10) NOT NULL,
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [NoRequired] [int] NULL,
    [UnitPrice] [money] NULL
,    CONSTRAINT [PK_tblTestReqBaseline_1__18] PRIMARY KEY CLUSTERED
    (
        Program, TestCode, Buyer
    )
) ON [PRIMARY]
GO
