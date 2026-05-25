USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdditionalCosts_Log](
    [SequenceNo] [int] IDENTITY(1,1) NOT NULL,
    [JobCode] [varchar](20) NOT NULL,
    [Account] [varchar](50) NOT NULL,
    [Description] [varchar](20) NOT NULL,
    [ItemCost] [money] NOT NULL,
    [Freq] [varchar](5) NULL,
    [Supplier] [varchar](50) NULL,
    [Date_Time] [datetime] NULL,
    [User_ID] [varchar](20) NULL,
    [Insert_Delete] [char](2) NULL
,    CONSTRAINT [PK_AdditionalCosts_Log] PRIMARY KEY NONCLUSTERED
    (
        SequenceNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Ind_Dt] ON [dbo].[AdditionalCosts_Log]
(
    Date_Time
)
GO
CREATE NONCLUSTERED INDEX [Ind_JC] ON [dbo].[AdditionalCosts_Log]
(
    JobCode
)
GO
