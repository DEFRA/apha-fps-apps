USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblAnimalReq](
    [Year] [smallint] NOT NULL,
    [JobCode] [varchar](20) NOT NULL,
    [AnimalType] [varchar](50) NOT NULL,
    [NumberOfDays] [float] NOT NULL,
    [NumberOfAnimals] [float] NOT NULL,
    [AR_Counter] [int] IDENTITY(1,1) NOT NULL
,    CONSTRAINT [PK_MY_tblAnimalReq] PRIMARY KEY CLUSTERED
    (
        AR_Counter
    )
) ON [PRIMARY]
GO
