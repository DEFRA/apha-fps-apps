USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblMTConversion](
    [OldProject] [nvarchar](20) NOT NULL,
    [OldCode] [nvarchar](50) NOT NULL,
    [NewProject] [nvarchar](20) NOT NULL,
    [NewCode] [nvarchar](50) NOT NULL,
    [Percentage] [float] NULL,
    [Hours] [float] NULL
,    CONSTRAINT [PK_tblMTConversion] PRIMARY KEY NONCLUSTERED
    (
        OldProject, OldCode, NewProject, NewCode
    )
) ON [PRIMARY]
GO
