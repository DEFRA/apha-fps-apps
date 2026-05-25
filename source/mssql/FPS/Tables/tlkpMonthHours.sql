USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpMonthHours](
    [Year] [smallint] NOT NULL,
    [Month] [smallint] NOT NULL,
    [Days] [numeric](5, 1) NULL,
    [CVLHours] [numeric](5, 1) NULL,
    [VIDHours] [numeric](5, 1) NULL,
    [FMonth] [smallint] NULL
) ON [PRIMARY]
GO
CREATE UNIQUE CLUSTERED INDEX [tlkpMonthHours_PK] ON [dbo].[tlkpMonthHours]
(
    Year, Month
)
GO
