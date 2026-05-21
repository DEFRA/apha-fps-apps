USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[sysdiagrams](
    [name] [sysname] NOT NULL,
    [principal_id] [int] NOT NULL,
    [diagram_id] [int] IDENTITY(1,1) NOT NULL,
    [version] [int] NULL,
    [definition] [varbinary](max) NULL
,    CONSTRAINT [PK__sysdiagrams__658C0CBD] PRIMARY KEY CLUSTERED
    (
        diagram_id
    )
,    CONSTRAINT [UK_principal_name] UNIQUE NONCLUSTERED
    (
        principal_id, name
    )
) ON [PRIMARY]
GO
