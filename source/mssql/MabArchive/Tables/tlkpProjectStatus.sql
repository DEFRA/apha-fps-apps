USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpProjectStatus](
    [ProjectStatus] [varchar](50) NOT NULL,
    [Is_FPS] [bit] NOT NULL,
    [Is_Pims] [bit] NOT NULL
,    CONSTRAINT [PK_tlkpProjectStatus] PRIMARY KEY NONCLUSTERED
    (
        ProjectStatus
    )
) ON [PRIMARY]
GO
