USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAnimalReq](
    [JobCode] [varchar](20) NOT NULL,
    [AnimalType] [varchar](50) NOT NULL,
    [NumberOfDays] [float] NOT NULL CONSTRAINT [DF__tblAnimal__Numbe__7088BE1D] DEFAULT (0),
    [NumberOfAnimals] [float] NOT NULL CONSTRAINT [DF__tblAnimal__Numbe__717CE256] DEFAULT (0),
    [IndCounter] [int] IDENTITY(1,1) NOT NULL
,    CONSTRAINT [PK__tblAnimalReq__7271068F] PRIMARY KEY CLUSTERED
    (
        IndCounter
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAnimalReq] WITH CHECK ADD CONSTRAINT [FK__tblAnimal__Anima__2FCF1A8A] FOREIGN KEY(AnimalType)
REFERENCES [dbo].[tblAnimals] (AnimalType)
GO
ALTER TABLE [dbo].[tblAnimalReq] CHECK CONSTRAINT [FK__tblAnimal__Anima__2FCF1A8A]
GO
ALTER TABLE [dbo].[tblAnimalReq] WITH CHECK ADD CONSTRAINT [FK__tblAnimal__JobCo__7147D82C] FOREIGN KEY(JobCode)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[tblAnimalReq] CHECK CONSTRAINT [FK__tblAnimal__JobCo__7147D82C]
GO
