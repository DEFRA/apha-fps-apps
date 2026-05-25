USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Milestone](
    [Project] [varchar](20) NOT NULL,
    [MilestoneRef] [varchar](4) NOT NULL,
    [ObjectiveRef] [varchar](50) NOT NULL,
    [MilsetoneTitle] [varchar](120) NULL,
    [PlanDate] [datetime] NULL,
    [ActualDate] [datetime] NULL,
    [Comment] [text] NULL,
    [MonthNoFin] [float] NULL,
    [Year] [varchar](50) NULL
,    CONSTRAINT [PK_Milestone_1__12] PRIMARY KEY CLUSTERED
    (
        Project, MilestoneRef, ObjectiveRef
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Milestone] WITH CHECK ADD CONSTRAINT [FK_Milestone_1__11] FOREIGN KEY(Project)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[Milestone] CHECK CONSTRAINT [FK_Milestone_1__11]
GO
