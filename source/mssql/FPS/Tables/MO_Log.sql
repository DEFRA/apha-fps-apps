USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MO_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [TestCode] [varchar](20) NULL,
    [Buyer] [varchar](20) NULL,
    [Month] [float] NULL,
    [WorkGroup] [varchar](50) NULL,
    [Volume] [float] NULL,
    [WGBuyer] [varchar](50) NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL
) ON [PRIMARY]
GO
