USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDB_Variables](
    [DB_Var_Name] [varchar](20) NOT NULL,
    [DB_Var_Value] [varchar](20) NULL
,    CONSTRAINT [PK_tblDB_Variables] PRIMARY KEY NONCLUSTERED
    (
        DB_Var_Name
    )
) ON [PRIMARY]
GO
