USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth](
    [Project] [varchar](20) NOT NULL,
    [MonthNo] [int] NOT NULL,
    [CostProfile] [money] NULL
,    CONSTRAINT [PK_ProjectMonth_1__16] PRIMARY KEY CLUSTERED
    (
        Project, MonthNo
    )
) ON [PRIMARY]
GO
