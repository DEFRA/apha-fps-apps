USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblAdditionalCosts](
    [Year] [smallint] NOT NULL,
    [JobCode] [varchar](20) NOT NULL,
    [Account] [varchar](50) NOT NULL,
    [Description] [varchar](20) NOT NULL,
    [ItemCost] [money] NOT NULL,
    [Freq] [varchar](5) NULL,
    [Supplier] [varchar](50) NULL,
    [AC_Counter] [int] IDENTITY(1,1) NOT NULL
,    CONSTRAINT [PK_MY_tblAdditionalCosts] PRIMARY KEY CLUSTERED
    (
        AC_Counter
    )
) ON [PRIMARY]
GO
