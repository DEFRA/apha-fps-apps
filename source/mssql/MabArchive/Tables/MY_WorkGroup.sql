USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_WorkGroup](
    [Year] [smallint] NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [CostCentre] [float] NULL,
    [Owner] [varchar](50) NULL,
    [Description] [varchar](45) NULL,
    [CentralOverhead] [money] NULL,
    [SendEmail] [tinyint] NULL,
    [COS90] [tinyint] NULL,
    [CostCentreOld] [float] NULL,
    [Email_Recipient] [varchar](50) NULL
,    CONSTRAINT [PK_MY_WorkGroup] PRIMARY KEY CLUSTERED
    (
        Year, WorkGroup
    )
) ON [PRIMARY]
GO
