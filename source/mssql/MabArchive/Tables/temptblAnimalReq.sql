USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblAnimalReq](
    [AR_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [int] NULL CONSTRAINT [DF__Temporary__Proje__5629CD9C] DEFAULT (0),
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__571DF1D5] DEFAULT (0),
    [AnimalType] [nvarchar](50) NULL,
    [Number of Days] [float] NULL CONSTRAINT [DF__Temporary__Numbe__5812160E] DEFAULT (0),
    [Number of Animals] [float] NULL CONSTRAINT [DF__Temporary__Numbe__59063A47] DEFAULT (0),
    [DailyRate] [float] NULL CONSTRAINT [DF__Temporary__Daily__59FA5E80] DEFAULT (0)
,    CONSTRAINT [aaaaatemptblAnimalReq_PK] PRIMARY KEY NONCLUSTERED
    (
        AR_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[temptblAnimalReq] WITH CHECK ADD CONSTRAINT [temptblAnimalReq_FK00] FOREIGN KEY(Year, Project)
REFERENCES [dbo].[temptblProjectYear] (YearNo, Project)
GO
ALTER TABLE [dbo].[temptblAnimalReq] CHECK CONSTRAINT [temptblAnimalReq_FK00]
GO
CREATE NONCLUSTERED INDEX [Proj_ind] ON [dbo].[temptblAnimalReq]
(
    Project, Year, AnimalType
)
GO
CREATE NONCLUSTERED INDEX [tblAnimalReqProject] ON [dbo].[temptblAnimalReq]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [temptblProjectYeartemptblAnimalReq] ON [dbo].[temptblAnimalReq]
(
    Project, Year
)
GO
