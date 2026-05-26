USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestReq_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [TestCode] [varchar](20) NULL,
    [Buyer] [varchar](20) NULL,
    [UnitPrice] [float] NULL,
    [NoRequired] [int] NULL,
    [ProjectBuyerCode] [varchar](50) NULL,
    [TestBuyerCode] [varchar](50) NULL,
    [Active] [tinyint] NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL,
    [JobCode] AS ([ProjectBuyerCode])
,    CONSTRAINT [PK_TestReq_Log] PRIMARY KEY NONCLUSTERED
    (
        SequenceNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Ind_Dt] ON [dbo].[TestReq_Log]
(
    Date_Time
)
GO
CREATE NONCLUSTERED INDEX [Ind_JC] ON [dbo].[TestReq_Log]
(
    JobCode
)
GO
