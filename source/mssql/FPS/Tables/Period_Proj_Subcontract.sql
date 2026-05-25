USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_Proj_Subcontract](
    [Period] [tinyint] NOT NULL,
    [SubContCounter] [int] NOT NULL,
    [Project] [varchar](20) NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [IsDefraProject] [varchar](3) NOT NULL,
    [OPC] [varchar](50) NULL,
    [OCC] [float] NULL,
    [Month] [float] NULL,
    [Amount] [money] NULL,
    [AcctCode] [varchar](30) NULL
,    CONSTRAINT [PK_Period_Proj_Subcontract] PRIMARY KEY CLUSTERED
    (
        Period, SubContCounter
    )
) ON [PRIMARY]
GO
