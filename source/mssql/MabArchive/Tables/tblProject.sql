USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProject](
    [Project] [varchar](50) NOT NULL,
    [PlanCat] [varchar](50) NULL,
    [ProjectTitle] [varchar](100) NULL,
    [Programme] [varchar](50) NULL,
    [ProjectWorkGroup] [varchar](50) NULL,
    [ContractPrice] [float] NULL,
    [StartDate] [datetime] NULL,
    [Disease] [varchar](50) NULL,
    [StartFYear] [float] NULL CONSTRAINT [DF__Temporary__Start__15502E78] DEFAULT (0),
    [Customer Name] [varchar](50) NULL,
    [Contract Number] [varchar](50) NULL,
    [SubmittedByFName] [varchar](50) NULL,
    [SubmittedByLName] [varchar](50) NULL,
    [Date of Submission] [datetime] NULL,
    [Prepared by] [varchar](50) NULL,
    [Inflation] [int] NULL CONSTRAINT [DF__Temporary__Infla__164452B1] DEFAULT (0),
    [FinancialYears] [int] NULL,
    [Notes] [varchar](255) NULL,
    [EuroConvRate] [float] NULL,
    [IsDefraProject] [smallint] NULL
,    CONSTRAINT [aaaaatblProject_PK] PRIMARY KEY NONCLUSTERED
    (
        Project
    )
) ON [PRIMARY]
GO
