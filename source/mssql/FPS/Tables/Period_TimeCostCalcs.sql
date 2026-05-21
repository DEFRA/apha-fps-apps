USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_TimeCostCalcs](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Period] [int] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [Month] [float] NOT NULL,
    [DefraProject] [varchar](3) NOT NULL,
    [OCC] [float] NULL,
    [OPC] [varchar](50) NULL,
    [SPC] [varchar](50) NOT NULL,
    [SCC] [float] NULL,
    [Name] [varchar](50) NULL,
    [GradeCode] [varchar](10) NULL,
    [SPNumber] [varchar](10) NOT NULL,
    [ChargeRate] [money] NULL,
    [Pay] [money] NULL,
    [Nonpay] [money] NULL,
    [Overhead] [money] NULL,
    [Time] [float] NULL,
    [TotalCost] [money] NULL
,    CONSTRAINT [PK_Period_TimeCostCalcs_1] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
