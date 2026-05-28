USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPeriodMonth](
    [EndMonth] [float] NOT NULL,
    [MonthNo] [float] NOT NULL
,    CONSTRAINT [aaaaatblkPeriodMonth_PK] PRIMARY KEY NONCLUSTERED
    (
        EndMonth, MonthNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MonthNo] ON [dbo].[tblPeriodMonth]
(
    MonthNo
)
GO
