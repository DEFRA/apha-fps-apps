USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecreateSummaries_Log](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [UserID] [varchar](20) NULL,
    [Period] [smallint] NULL,
    [DateDone] [datetime] NULL
,    CONSTRAINT [PK_RecreateSummaries_Log] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
