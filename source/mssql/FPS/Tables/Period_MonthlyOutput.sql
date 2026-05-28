USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_MonthlyOutput](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Period] [int] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [IsDefraProject] [varchar](3) NOT NULL,
    [OPC] [varchar](50) NULL,
    [OCC] [float] NULL,
    [Month] [float] NOT NULL,
    [SPC] [varchar](50) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [SCC] [float] NULL,
    [TestCode] [varchar](20) NOT NULL,
    [Volume] [float] NULL,
    [TestPrice] [money] NULL,
    [TotalCost] [money] NULL
,    CONSTRAINT [PK_Period_MonthlyOutput_1] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
