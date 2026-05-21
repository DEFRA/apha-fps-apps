USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSurvFF_Fees](
    [PactCode] [varchar](50) NOT NULL,
    [Owning_VIC] [varchar](50) NOT NULL,
    [Received] [datetime] NULL,
    [Contract] [varchar](20) NOT NULL,
    [Record_ID] [varchar](20) NOT NULL,
    [Volume] [float] NULL,
    [TotalFee] [money] NULL
,    CONSTRAINT [PK_tblSurvFF_Fees] PRIMARY KEY NONCLUSTERED
    (
        Owning_VIC, Contract, Record_ID
    )
) ON [PRIMARY]
GO
