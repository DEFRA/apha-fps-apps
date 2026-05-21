USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeCostCalcs](
    [WorkGroup] [varchar](50) NOT NULL,
    [JobCode] [varchar](50) NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [Month] [float] NOT NULL,
    [StaffID] [varchar](50) NOT NULL,
    [GradeCode] [varchar](10) NULL,
    [Name] [varchar](50) NULL,
    [ChargeRate] [money] NULL,
    [Class] [varchar](255) NULL,
    [Time] [float] NULL,
    [Cost] [float] NULL,
    [Division] [varchar](10) NULL,
    [JobCodeOld] [varchar](14) NULL,
    [Pay] [money] NULL,
    [NonPay] [money] NULL,
    [Overhead] [money] NULL
,    CONSTRAINT [aaaaaTimeCostCalcs_PK] PRIMARY KEY NONCLUSTERED
    (
        WorkGroup, JobCode, Project, Month, StaffID
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Class] ON [dbo].[TimeCostCalcs]
(
    Class
)
GO
CREATE NONCLUSTERED INDEX [Project] ON [dbo].[TimeCostCalcs]
(
    Project
)
GO
