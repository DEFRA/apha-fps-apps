USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDBVariables](
    [DB_Variable] [nvarchar](50) NOT NULL,
    [NVal] [float] NULL CONSTRAINT [DF__TemporaryU__NVal__1FCDBCEB] DEFAULT (0)
,    CONSTRAINT [aaaaatblDBVariables_PK] PRIMARY KEY NONCLUSTERED
    (
        DB_Variable
    )
) ON [PRIMARY]
GO
