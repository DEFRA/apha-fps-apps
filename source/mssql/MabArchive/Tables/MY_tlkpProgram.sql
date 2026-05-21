USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tlkpProgram](
    [Year] [smallint] NOT NULL,
    [ProgramNo] [varchar](10) NOT NULL,
    [ProgramName] [varchar](80) NULL,
    [Directorate] [varchar](15) NULL,
    [MINIM] [varchar](7) NULL,
    [SECTOR_NAME] [varchar](50) NULL,
    [CUSTOMER] [varchar](50) NULL,
    [Target] [money] NULL,
    [Manager] [varchar](50) NULL
,    CONSTRAINT [PK_MY_tlkpProgram] PRIMARY KEY CLUSTERED
    (
        Year, ProgramNo
    )
) ON [PRIMARY]
GO
