USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthCasework](
    [Project] [varchar](20) NOT NULL,
    [MonthNo] [int] NOT NULL,
    [CWDebit] [float] NULL,
    [CWCredit] [float] NULL
,    CONSTRAINT [PK_ProjectMonthCasework_1__10] PRIMARY KEY CLUSTERED
    (
        Project, MonthNo
    )
) ON [PRIMARY]
GO
