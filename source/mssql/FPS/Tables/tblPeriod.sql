USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPeriod](
    [PeriodName] [varchar](50) NOT NULL,
    [PeriodType] [varchar](50) NULL,
    [StartPeriod] [float] NULL,
    [EndPeriod] [float] NULL,
    [FinalSummariesRun] [smallint] NULL,
    [PeriodLocked] [smallint] NOT NULL CONSTRAINT [DF_tblPeriod_PeriodLocked] DEFAULT ((0))
,    CONSTRAINT [aaaaatblPeriod_PK] PRIMARY KEY NONCLUSTERED
    (
        PeriodName
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [EndPeriod] ON [dbo].[tblPeriod]
(
    EndPeriod
)
GO
