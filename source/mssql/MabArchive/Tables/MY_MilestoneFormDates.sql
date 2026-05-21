USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_MilestoneFormDates](
    [Year] [smallint] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [Jan] [smalldatetime] NULL,
    [Feb] [smalldatetime] NULL,
    [Mar] [smalldatetime] NULL,
    [Apr] [smalldatetime] NULL,
    [May] [smalldatetime] NULL,
    [Jun] [smalldatetime] NULL,
    [Jul] [smalldatetime] NULL,
    [Aug] [smalldatetime] NULL,
    [Sep] [smalldatetime] NULL,
    [Oct] [smalldatetime] NULL,
    [Nov] [smalldatetime] NULL,
    [Dec] [smalldatetime] NULL
,    CONSTRAINT [PK_MY_MilestoneFormDates] PRIMARY KEY CLUSTERED
    (
        Year, ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MY_MilestoneFormDates] WITH CHECK ADD CONSTRAINT [FK_MY_MilestoneFormDates_G_tlkpProject_RadTrackData] FOREIGN KEY(ParentProject)
REFERENCES [dbo].[G_tlkpProject_RadTrackData] (ParentProject)
GO
ALTER TABLE [dbo].[MY_MilestoneFormDates] CHECK CONSTRAINT [FK_MY_MilestoneFormDates_G_tlkpProject_RadTrackData]
GO
