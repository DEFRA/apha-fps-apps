USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTestReqWG](
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [Workgroup] [varchar](50) NOT NULL,
    [Amount] [int] NULL CONSTRAINT [DF_tblTestReqWG_Amount] DEFAULT (0)
,    CONSTRAINT [PK_tblTestReqWG] PRIMARY KEY NONCLUSTERED
    (
        TestCode, Buyer, Workgroup
    )
) ON [PRIMARY]
GO
