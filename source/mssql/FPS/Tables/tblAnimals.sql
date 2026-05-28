USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAnimals](
    [AnimalType] [varchar](50) NOT NULL,
    [Species] [varchar](50) NULL,
    [Security_Level] [varchar](50) NULL,
    [DailyRate] [money] NULL,
    [PlanByWeek] [bit] NOT NULL CONSTRAINT [DF_tblAnimals_PlanByWeek] DEFAULT (0),
    [DefraDailyRate] [money] NULL
,    CONSTRAINT [PK__tblAnimals__18EBB532] PRIMARY KEY CLUSTERED
    (
        AnimalType
    )
) ON [PRIMARY]
GO
