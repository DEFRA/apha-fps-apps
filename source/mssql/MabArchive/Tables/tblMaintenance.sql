USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblMaintenance](
    [Formname] [varchar](50) NOT NULL,
    [Description] [varchar](50) NULL,
    [UserNotes] [varchar](250) NULL,
    [Obsolete?] [bit] NOT NULL,
    [DisplaySeq] [int] NULL
,    CONSTRAINT [PK_tblMaintenance] PRIMARY KEY CLUSTERED
    (
        Formname
    )
) ON [PRIMARY]
GO
