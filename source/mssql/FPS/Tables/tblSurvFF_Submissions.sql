USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSurvFF_Submissions](
    [SD_Pact_WG] [varchar](50) NOT NULL,
    [Contract] [varchar](20) NOT NULL,
    [CountOfJobName] [int] NULL
,    CONSTRAINT [PK___1__12] PRIMARY KEY CLUSTERED
    (
        SD_Pact_WG, Contract
    )
) ON [PRIMARY]
GO
