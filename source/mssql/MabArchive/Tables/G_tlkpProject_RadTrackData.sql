USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[G_tlkpProject_RadTrackData](
    [ParentProject] [varchar](20) NOT NULL,
    [Version] [varchar](10) NULL,
    [FileRef] [varchar](20) NULL,
    [CustomerRef] [varchar](20) NULL,
    [StartDate] [datetime] NULL,
    [EndDate] [datetime] NULL,
    [FinalReportReceived] [datetime] NULL,
    [FinalReportSent] [datetime] NULL,
    [Inflation] [smallint] NULL CONSTRAINT [DF_G_tlkpProject_RadTrackData_Inflation] DEFAULT ((0)),
    [ClosedDate] [datetime] NULL,
    [UseProjectYear] [smallint] NOT NULL CONSTRAINT [DF_G_tlkpProject_RadTrackData_UseProjectYear] DEFAULT ((0)),
    [Status] [varchar](50) NULL,
    [PCForecastSpend] [float] NULL,
    [RiskID] [int] NULL,
    [CostbookNumber] [varchar](10) NULL,
    [RevisedEndDate] [datetime] NULL,
    [FormRequired] [bit] NOT NULL CONSTRAINT [DF_G_tlkpProject_RadTrackData_FormRequired] DEFAULT ((1)),
    [OverallCustIncome] [money] NULL
,    CONSTRAINT [PK_G_tlkpProject_RadTrackData] PRIMARY KEY CLUSTERED
    (
        ParentProject
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[G_tlkpProject_RadTrackData] WITH CHECK ADD CONSTRAINT [FK_G_tlkpProject_RadTrackData_tlkpRisk] FOREIGN KEY(RiskID)
REFERENCES [dbo].[tlkpRisk] (RiskID)
GO
ALTER TABLE [dbo].[G_tlkpProject_RadTrackData] CHECK CONSTRAINT [FK_G_tlkpProject_RadTrackData_tlkpRisk]
GO
