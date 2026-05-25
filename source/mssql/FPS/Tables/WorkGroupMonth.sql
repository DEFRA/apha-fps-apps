USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkGroupMonth](
    [WorkGroup] [varchar](50) NOT NULL,
    [Month] [float] NOT NULL,
    [RunningCost] [money] NULL,
    [RunCostProfile] [money] NULL
) ON [PRIMARY]
GO
