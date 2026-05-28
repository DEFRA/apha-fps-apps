USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_MonthlyOutput](
    [Year] [smallint] NOT NULL,
    [TestCode] [varchar](20) NOT NULL,
    [Buyer] [varchar](20) NOT NULL,
    [Month] [float] NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [Volume] [float] NULL,
    [WGBuyer] [varchar](50) NULL
,    CONSTRAINT [PK_MY_MonthlyOutput] PRIMARY KEY CLUSTERED
    (
        Year, TestCode, Buyer, Month, WorkGroup
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [MY_MO_Month] ON [dbo].[MY_MonthlyOutput]
(
    Month
)
GO
CREATE NONCLUSTERED INDEX [MY_MO_Testcode] ON [dbo].[MY_MonthlyOutput]
(
    TestCode
)
GO
CREATE NONCLUSTERED INDEX [MY_MO_Year] ON [dbo].[MY_MonthlyOutput]
(
    Year
)
GO
