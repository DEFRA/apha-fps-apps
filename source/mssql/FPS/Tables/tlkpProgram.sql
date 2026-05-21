USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpProgram](
    [ProgramNo] [varchar](10) NOT NULL,
    [ProgramName] [varchar](80) NULL,
    [Directorate] [varchar](15) NULL,
    [MINIM] [varchar](7) NULL,
    [SECTOR_NAME] [varchar](50) NULL CONSTRAINT [DF_tlkpProgram_SECTOR_NAME] DEFAULT ('Charge'),
    [CUSTOMER] [varchar](50) NULL,
    [Target] [money] NULL CONSTRAINT [DF__tlkpProgr__Targe__208CD6FA] DEFAULT (0),
    [Manager] [varchar](50) NULL
,    CONSTRAINT [PK__tlkpProgram__2180FB33] PRIMARY KEY CLUSTERED
    (
        ProgramNo
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [tlkpProgram_MINIM] ON [dbo].[tlkpProgram]
(
    MINIM
)
GO
