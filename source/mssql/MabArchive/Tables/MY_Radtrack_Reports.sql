USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_Radtrack_Reports](
    [Year] [smallint] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [Type] [varchar](10) NOT NULL,
    [Reminder1] [datetime] NULL,
    [Reminder2] [datetime] NULL,
    [ReplyReceived] [datetime] NULL,
    [SentToProgManager] [datetime] NULL,
    [SentToProjLeader] [datetime] NULL,
    [EmailedToCustomer] [datetime] NULL,
    [SignedCopyToCustomer] [datetime] NULL,
    [RepDueDate] [datetime] NULL,
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [ReportAgreedDate] [datetime] NULL
,    CONSTRAINT [PK_MY_Radtrack_Reports] PRIMARY KEY NONCLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MY_Radtrack_Reports] WITH CHECK ADD CONSTRAINT [FK_MY_Radtrack_Reports_G_tlkpProject_RadTrackData] FOREIGN KEY(Project)
REFERENCES [dbo].[G_tlkpProject_RadTrackData] (ParentProject)
GO
ALTER TABLE [dbo].[MY_Radtrack_Reports] CHECK CONSTRAINT [FK_MY_Radtrack_Reports_G_tlkpProject_RadTrackData]
GO
