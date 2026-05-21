USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAnimalReq](
    [AR_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](50) NULL,
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__286302EC] DEFAULT (0),
    [AnimalType] [nvarchar](50) NOT NULL,
    [Number of Days] [float] NULL CONSTRAINT [DF__Temporary__Numbe__29572725] DEFAULT (0),
    [Number of Animals] [float] NULL CONSTRAINT [DF__Temporary__Numbe__2A4B4B5E] DEFAULT (0),
    [DailyRate] [float] NULL CONSTRAINT [DF__Temporary__Daily__2B3F6F97] DEFAULT (0)
,    CONSTRAINT [aaaaatblAnimalReq_PK] PRIMARY KEY NONCLUSTERED
    (
        AR_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAnimalReq] WITH CHECK ADD CONSTRAINT [tblAnimalReq_FK00] FOREIGN KEY(Project, Year)
REFERENCES [dbo].[tblProjectYear] (Project, YearNo)
GO
ALTER TABLE [dbo].[tblAnimalReq] CHECK CONSTRAINT [tblAnimalReq_FK00]
GO
CREATE NONCLUSTERED INDEX [Proj_ind] ON [dbo].[tblAnimalReq]
(
    Project, Year, AnimalType
)
GO
CREATE NONCLUSTERED INDEX [tblAnimalReqProject] ON [dbo].[tblAnimalReq]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [tblProjectYeartblAnimalReq] ON [dbo].[tblAnimalReq]
(
    Project, Year
)
GO
