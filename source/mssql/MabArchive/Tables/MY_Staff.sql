USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_Staff](
    [Year] [smallint] NOT NULL,
    [StaffID] [varchar](50) NOT NULL,
    [WorkGroupGrade] [varchar](50) NOT NULL,
    [Name] [varchar](50) NOT NULL,
    [Title] [varchar](4) NULL,
    [PersonStatus] [varchar](10) NULL,
    [PersonClass] [varchar](10) NULL,
    [HrsPaid] [float] NULL,
    [Leave] [float] NULL,
    [SickSpecial] [float] NULL,
    [HrsAvail] [float] NULL
,    CONSTRAINT [PK_MY_Staff] PRIMARY KEY CLUSTERED
    (
        Year, StaffID
    )
) ON [PRIMARY]
GO
