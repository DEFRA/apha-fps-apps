USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpYear](
    [Year] [int] NOT NULL,
    [LatestMonthReleased] [int] NULL
,    CONSTRAINT [PK_tlkpYear] PRIMARY KEY CLUSTERED
    (
        Year
    )
) ON [PRIMARY]
GO
