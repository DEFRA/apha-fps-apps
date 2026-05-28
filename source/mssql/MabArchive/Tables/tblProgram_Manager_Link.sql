USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProgram_Manager_Link](
    [Program] [varchar](50) NOT NULL,
    [Manager] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblProgram_Manager] PRIMARY KEY CLUSTERED
    (
        Program, Manager
    )
) ON [PRIMARY]
GO
