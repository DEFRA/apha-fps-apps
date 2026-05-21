USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCBSummary](
    [CBProject] [varchar](50) NOT NULL,
    [FinancialYear] [smallint] NOT NULL,
    [CBProjectTitle] [varchar](100) NULL,
    [StartDate] [datetime] NOT NULL,
    [AnimalCost] [money] NULL,
    [TestCost] [money] NULL,
    [StaffCost] [money] NULL,
    [LineCost] [money] NULL
,    CONSTRAINT [PK_tblCBSummary_1__10] PRIMARY KEY CLUSTERED
    (
        CBProject, FinancialYear
    )
) ON [PRIMARY]
GO
