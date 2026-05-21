USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblProject](
    [Project] [int] NOT NULL CONSTRAINT [DF__Temporary__Proje__49C3F6B7] DEFAULT (0),
    [Programme] [nvarchar](10) NULL,
    [PlanCat] [nvarchar](50) NULL,
    [ProjectTitle] [nvarchar](100) NULL,
    [ProjectWorkGroup] [nvarchar](50) NULL,
    [ContractPrice] [float] NULL,
    [StartDate] [datetime] NULL,
    [Disease] [nvarchar](50) NULL,
    [StartFYear] [float] NULL CONSTRAINT [DF__Temporary__Start__4AB81AF0] DEFAULT (0),
    [Customer Name] [nvarchar](50) NULL,
    [Contract Number] [nvarchar](50) NULL,
    [Submitted by] [nvarchar](50) NULL,
    [Date of Submission] [datetime] NULL,
    [Prepared by] [nvarchar](50) NULL,
    [Inflation] [bit] NULL CONSTRAINT [DF__Temporary__Infla__4BAC3F29] DEFAULT (0),
    [Ready] [bit] NULL CONSTRAINT [DF__Temporary__Ready__4CA06362] DEFAULT (0),
    [FinancialYears] [bit] NULL CONSTRAINT [DF_temptblProject_FinancialYears] DEFAULT (1),
    [Notes] [nvarchar](1000) NULL
,    CONSTRAINT [aaaaatemptblProject_PK] PRIMARY KEY NONCLUSTERED
    (
        Project
    )
) ON [PRIMARY]
GO
