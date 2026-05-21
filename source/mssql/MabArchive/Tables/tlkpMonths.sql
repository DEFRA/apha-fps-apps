USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpMonths](
    [FMonthNo] [int] NOT NULL,
    [MonthNo] [int] NULL,
    [MonthName] [varchar](50) NULL
,    CONSTRAINT [PK_tlkpMonths] PRIMARY KEY NONCLUSTERED
    (
        FMonthNo
    )
) ON [PRIMARY]
GO
