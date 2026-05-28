USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDisease](
    [Disease] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblDisease] PRIMARY KEY CLUSTERED
    (
        Disease
    )
) ON [PRIMARY]
GO
