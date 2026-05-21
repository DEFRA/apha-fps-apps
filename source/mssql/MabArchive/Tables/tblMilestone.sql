USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblMilestone](
    [Project] [varchar](20) NOT NULL,
    [Number] [varchar](10) NOT NULL,
    [Description] [varchar](500) NULL,
    [DateDue] [datetime] NOT NULL,
    [DateCompleted] [datetime] NULL,
    [DateFormReceived] [datetime] NULL,
    [UnderSDReview] [smallint] NULL CONSTRAINT [DF_tblMilestone_UnderSDReview] DEFAULT (0),
    [OnTarget] [smallint] NULL CONSTRAINT [DF_tblMilestone_OnTarget] DEFAULT (0),
    [ProjectLeaderComment] [varchar](max) NULL,
    [CAPSComment] [varchar](250) NULL,
    [IDType] [char](1) NULL
,    CONSTRAINT [PK_tblMilestone] PRIMARY KEY NONCLUSTERED
    (
        Project, Number
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblMilestone] WITH CHECK ADD CONSTRAINT [FK_tblMilestone_G_tlkpProject_RadTrackData] FOREIGN KEY(Project)
REFERENCES [dbo].[G_tlkpProject_RadTrackData] (ParentProject)
GO
ALTER TABLE [dbo].[tblMilestone] CHECK CONSTRAINT [FK_tblMilestone_G_tlkpProject_RadTrackData]
GO
ALTER TABLE [dbo].[tblMilestone] WITH CHECK ADD CONSTRAINT [FK_tblMilestone_tlkpMilestoneType] FOREIGN KEY(IDType)
REFERENCES [dbo].[tlkpMilestoneType] (IDType)
GO
ALTER TABLE [dbo].[tblMilestone] CHECK CONSTRAINT [FK_tblMilestone_tlkpMilestoneType]
GO
