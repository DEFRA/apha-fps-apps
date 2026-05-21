USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblReportGroup](
    [GroupID] [int] IDENTITY(1,1) NOT NULL,
    [Description] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblReportGroup] PRIMARY KEY CLUSTERED
    (
        GroupID
    )
) ON [PRIMARY]
GO
