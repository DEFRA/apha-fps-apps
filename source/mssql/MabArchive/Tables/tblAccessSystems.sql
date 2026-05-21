USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAccessSystems](
    [SystemID] [int] NOT NULL,
    [SystemName] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblAccessSystems] PRIMARY KEY NONCLUSTERED
    (
        SystemID
    )
) ON [PRIMARY]
GO
