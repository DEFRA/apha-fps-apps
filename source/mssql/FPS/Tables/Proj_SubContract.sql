USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Proj_SubContract](
    [SubContCounter] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](20) NULL,
    [TestJob] [varchar](50) NULL,
    [Month] [float] NULL,
    [Amount] [money] NULL,
    [WorkGroup] [varchar](50) NULL,
    [AcctCode] [varchar](30) NULL,
    [TimeStamp] [timestamp] NOT NULL,
    [Supplier] [varchar](50) NULL,
    [Description] [varchar](255) NULL,
    [SupplierNumber] [int] NULL,
    [DailyRate] [money] NULL,
    [AnimalDays] [int] NULL
,    CONSTRAINT [PK_Proj_SubContract_1__13] PRIMARY KEY CLUSTERED
    (
        SubContCounter
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Proj_SubContract] WITH CHECK ADD CONSTRAINT [FK_Proj_SubContract_1__11] FOREIGN KEY(Project)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[Proj_SubContract] CHECK CONSTRAINT [FK_Proj_SubContract_1__11]
GO
