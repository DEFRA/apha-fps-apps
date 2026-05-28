USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tlkpProjectRadTrackData](
    [Year] [smallint] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [BFBudget] [money] NULL,
    [PYBudget] [money] NULL,
    [Seedcorn] [money] NULL,
    [ManHours] [float] NULL,
    [ManDays] [float] NULL,
    [ManYears] [float] NULL,
    [PayCosts] [money] NULL,
    [NonPayOHCosts] [money] NULL,
    [TestCosts] [money] NULL,
    [AnimalCosts] [money] NULL,
    [NonAnimalCosts] [money] NULL,
    [ManHoursChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_ManHoursChanged] DEFAULT (0),
    [PayCostsChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_PayCostsChanged] DEFAULT (0),
    [NonPayOHCostsChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_NonPayOHCostsChanged] DEFAULT (0),
    [TestCostsChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_TestCostsChanged] DEFAULT (0),
    [AnimalCostsChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_AnimalCostsChanged] DEFAULT (0),
    [NonAnimalCostsChanged] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_NonAnimalCostsChanged] DEFAULT (0),
    [Adjustment] [money] NULL,
    [AdjustmentComment] [varchar](250) NULL,
    [Locked] [smallint] NULL CONSTRAINT [DF_MY_tlkpProjectRadTrackData_Locked] DEFAULT (0),
    [DateCosted] [datetime] NULL,
    [CostedBy] [varchar](20) NULL,
    [ActualExpenditure] [money] NULL,
    [ActualManYears] [float] NULL,
    [VLA_Budget] [money] NULL
,    CONSTRAINT [PK_MY_tlkpProjectRadTrackData] PRIMARY KEY CLUSTERED
    (
        Year, Project
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MY_tlkpProjectRadTrackData] WITH CHECK ADD CONSTRAINT [FK_MY_tlkpProjectRadTrackData_G_tlkpProject_RadTrackData] FOREIGN KEY(Project)
REFERENCES [dbo].[G_tlkpProject_RadTrackData] (ParentProject)
GO
ALTER TABLE [dbo].[MY_tlkpProjectRadTrackData] CHECK CONSTRAINT [FK_MY_tlkpProjectRadTrackData_G_tlkpProject_RadTrackData]
GO
