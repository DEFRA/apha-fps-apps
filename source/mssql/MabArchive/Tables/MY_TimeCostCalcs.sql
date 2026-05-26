USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_TimeCostCalcs](
    [Year] [smallint] NOT NULL,
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
,    CONSTRAINT [PK_MY_TimeCostCalcs] PRIMARY KEY CLUSTERED
    (
        Year, WorkGroup, JobCode, Project, Month, StaffID
    )
) ON [PRIMARY]
GO
