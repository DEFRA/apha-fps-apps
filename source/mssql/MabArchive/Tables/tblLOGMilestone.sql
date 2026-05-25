USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLOGMilestone](
    [Project] [varchar](20) NULL,
    [Number] [varchar](10) NULL,
    [Description] [varchar](500) NULL,
    [DateDue] [datetime] NULL,
    [DateCompleted] [datetime] NULL,
    [DateFormReceived] [datetime] NULL,
    [UnderSDReview] [smallint] NULL,
    [OnTarget] [smallint] NULL,
    [ProjectLeaderComment] [varchar](max) NULL,
    [CAPSComment] [varchar](250) NULL,
    [IDType] [char](1) NULL,
    [DateChanged] [datetime] NULL,
    [ChangedBy] [varchar](10) NULL,
    [UpdateType] [char](1) NULL,
    [ID] [int] IDENTITY(1,1) NOT NULL
,    CONSTRAINT [PK_LOG_tblMilestone] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
