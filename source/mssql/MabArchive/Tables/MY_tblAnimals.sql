USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblAnimals](
    [Year] [smallint] NOT NULL,
    [AnimalType] [varchar](50) NOT NULL,
    [Species] [varchar](50) NULL,
    [Security_Level] [varchar](50) NULL,
    [DailyRate] [money] NULL,
    [PlanByWeek] [bit] NULL,
    [DefraDailyRate] [money] NULL
,    CONSTRAINT [PK__MY_tblAnimals__18EBB532] PRIMARY KEY CLUSTERED
    (
        Year, AnimalType
    )
) ON [PRIMARY]
GO
