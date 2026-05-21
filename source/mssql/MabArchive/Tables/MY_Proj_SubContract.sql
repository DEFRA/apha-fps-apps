USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_Proj_SubContract](
    [Year] [smallint] NOT NULL,
    [SubContCounter] [int] NOT NULL,
    [Project] [varchar](20) NULL,
    [TestJob] [varchar](50) NULL,
    [Month] [float] NULL,
    [Amount] [money] NULL,
    [WorkGroup] [varchar](50) NULL,
    [AcctCode] [varchar](30) NULL,
    [Supplier] [varchar](50) NULL,
    [Description] [varchar](255) NULL,
    [SupplierNumber] [int] NULL,
    [DailyRate] [money] NULL,
    [AnimalDays] [int] NULL
,    CONSTRAINT [PK_MY_Proj_SubContract] PRIMARY KEY CLUSTERED
    (
        Year, SubContCounter
    )
) ON [PRIMARY]
GO
