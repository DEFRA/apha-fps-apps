USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_MonthlyTime](
    [Year] [smallint] NOT NULL,
    [PACTStaffID] [varchar](50) NOT NULL,
    [TimeCode] [varchar](50) NOT NULL,
    [Month] [float] NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [WorkGroup] [varchar](50) NULL,
    [Hours] [float] NULL
,    CONSTRAINT [PK_MY_MonthlyTime] PRIMARY KEY CLUSTERED
    (
        Year, PACTStaffID, TimeCode, Month, ParentProject
    )
) ON [PRIMARY]
GO
